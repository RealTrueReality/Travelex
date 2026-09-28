using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.ClientModel;
using Microsoft.Extensions.Logging;

namespace Travelex.Services;

/// <summary>
/// Personal-use Qwen client. A distributed app must use a backend proxy instead of a shared API key.
/// </summary>
public sealed class QwenService : IDisposable {
    private const string ApiKeyStorageKey = "Travelex.Qwen.ApiKey";
    private const string ModelStorageKey = "Travelex.Ai.Model";
    private const string MafPreviewStorageKey = "Travelex.Ai.UseMafPreview";
    private const string CompletionUrl = "https://maas.qianwenaiapi.com/compatible-mode/v1/chat/completions";
    public const string QwenModel = "qwen3.8-max";
    public const string DeepSeekModel = "deepseek-v4-pro-0813";
    private const string SystemPrompt = "你是 Travelex 的旅行开支分析助手。只根据用户提供的数据分析总支出、分类占比、时间与地点趋势，并提出具体可行的节省建议。金额和日期必须准确；数据不足时明确说明，不要编造实时天气、汇率、景点信息或声称使用了搜索工具。请用简洁中文回答。";

    private readonly HttpClient _httpClient = new();
    private readonly ILogger<QwenService> _logger;
    private readonly MafTravelAgentService _mafAgent;

    public QwenService(ILogger<QwenService> logger, MafTravelAgentService mafAgent) {
        _logger = logger;
        _mafAgent = mafAgent;
    }

    public async Task<bool> HasApiKeyAsync() =>
        !string.IsNullOrWhiteSpace(await SecureStorage.Default.GetAsync(ApiKeyStorageKey));

    public Task SaveApiKeyAsync(string apiKey) {
        var normalized = apiKey.Trim().Replace("\\_", "_", StringComparison.Ordinal);
        if (!normalized.StartsWith("sk-ws-", StringComparison.Ordinal) || normalized.Any(char.IsWhiteSpace)) {
            throw new ArgumentException("请输入千问 AI 平台的 sk-ws- API Key。", nameof(apiKey));
        }

        return SecureStorage.Default.SetAsync(ApiKeyStorageKey, normalized);
    }

    public void RemoveApiKey() => SecureStorage.Default.Remove(ApiKeyStorageKey);

    public string GetSelectedModel() {
        var model = Preferences.Default.Get(ModelStorageKey, QwenModel);
        return IsSupportedModel(model) ? model : QwenModel;
    }

    public void SetSelectedModel(string model) {
        if (!IsSupportedModel(model)) throw new ArgumentException("不支持的 AI 模型。", nameof(model));
        Preferences.Default.Set(ModelStorageKey, model);
    }

    private static bool IsSupportedModel(string model) => model is QwenModel or DeepSeekModel;

    public bool GetUseMafPreview() => Preferences.Default.Get(MafPreviewStorageKey, false);

    public void SetUseMafPreview(bool enabled) => Preferences.Default.Set(MafPreviewStorageKey, enabled);

    public async Task<string> AnalyzeTravelExpensesAsync(object travelData) {
        var apiKey = await SecureStorage.Default.GetAsync(ApiKeyStorageKey);
        if (string.IsNullOrWhiteSpace(apiKey)) return "请先在 AI 助手中设置 API Key。";

        var model = GetSelectedModel();
        using var request = CreateRequest(apiKey, model, travelData, stream: false);
        try {
            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return await GetErrorMessageAsync(response, model);

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (document.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String) {
                return content.GetString() ?? "分析结果为空，请稍后重试。";
            }

            return "分析结果格式异常，请稍后重试。";
        }
        catch (HttpRequestException) {
            return "网络请求失败，请检查连接后重试。";
        }
        catch (TaskCanceledException) {
            return "请求超时，请稍后重试。";
        }
        catch (JsonException) {
            return "服务返回的数据格式异常，请稍后重试。";
        }
    }

    public async IAsyncEnumerable<string> AnalyzeTravelExpensesStreamAsync(object travelData,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var apiKey = await SecureStorage.Default.GetAsync(ApiKeyStorageKey);
        if (string.IsNullOrWhiteSpace(apiKey)) {
            yield return "请先在 AI 助手中设置 API Key。";
            yield break;
        }
        var model = GetSelectedModel();
        var useMafPreview = GetUseMafPreview();

        // Android's native HTTP handler may perform network I/O while synchronously closing a
        // response stream. Keep the entire read AND disposal path off the Blazor UI thread.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(64) {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });
        var producer = Task.Run(() => ProduceStreamAsync(apiKey, model, travelData, useMafPreview,
            channel.Writer, linkedCts.Token));

        try {
            await foreach (var chunk in channel.Reader.ReadAllAsync(linkedCts.Token)) {
                yield return chunk;
            }
        }
        finally {
            if (!producer.IsCompleted) {
                // Cancellation can disconnect Android's native HTTP connection; do that off UI too.
                await Task.Run(linkedCts.Cancel);
            }
            await producer.ConfigureAwait(false);
        }
    }

    private async Task ProduceStreamAsync(string apiKey, string model, object travelData, bool useMafPreview,
        ChannelWriter<string> writer,
        CancellationToken cancellationToken) {
        var state = new StreamReadState();
        try {
            if (useMafPreview) {
                var receivedContent = false;
                await foreach (var chunk in _mafAgent.AnalyzeStreamAsync(apiKey, model, travelData, cancellationToken)
                                   .ConfigureAwait(false)) {
                    receivedContent = true;
                    await writer.WriteAsync(chunk, cancellationToken).ConfigureAwait(false);
                }
                if (!receivedContent) {
                    await writer.WriteAsync("没有收到分析结果，请稍后重试。", cancellationToken)
                        .ConfigureAwait(false);
                }
                state.Completed = true;
            }
            else {
                await ReadStreamAsync(apiKey, model, travelData, writer, state, cancellationToken)
                    .ConfigureAwait(false);
            }
            writer.TryComplete();
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested) {
            writer.TryComplete(ex);
        }
        catch (ClientResultException ex) when (useMafPreview) {
            // SDK exception text can include provider response details. Only expose the HTTP status.
            _logger.LogWarning("MAF request rejected: model {Model}, HTTP {StatusCode}", model, ex.Status);
            try {
                var message = ex.Status > 0
                    ? $"AI 请求失败（HTTP {ex.Status}）。请检查平台余额、模型权限与 Key。"
                    : "AI 网络请求失败，请检查连接后重试。";
                await writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
            }
            finally {
                writer.TryComplete();
            }
        }
        catch (Exception ex) {
            // The answer is already complete if only Android's connection cleanup failed.
            if (state.Completed) {
                _logger.LogWarning(ex, "Qwen response cleanup failed after completion");
                writer.TryComplete();
            }
            else {
                if (useMafPreview) {
                    // Do not leak provider response bodies through logs or UI exceptions.
                    _logger.LogError("MAF stream failed before completion: {ExceptionType}", ex.GetType().Name);
                    writer.TryComplete(new IOException("MAF preview stream failed."));
                }
                else {
                    _logger.LogError(ex, "Qwen stream failed before completion");
                    writer.TryComplete(ex);
                }
            }
        }
    }

    private async Task ReadStreamAsync(string apiKey, string model, object travelData, ChannelWriter<string> writer,
        StreamReadState state, CancellationToken cancellationToken) {
        using var request = CreateRequest(apiKey, model, travelData, stream: true);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) {
            var errorMessage = await GetErrorMessageAsync(response, model, cancellationToken).ConfigureAwait(false);
            await writer.WriteAsync(errorMessage, cancellationToken).ConfigureAwait(false);
            state.Completed = true;
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        var receivedContent = false;

        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line) {
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
            var data = line[5..].Trim();
            if (data == "[DONE]") {
                state.Completed = true;
                break;
            }
            if (data.Length == 0) continue;

            string? content;
            try {
                using var document = JsonDocument.Parse(data);
                content = GetDeltaContent(document.RootElement);
            }
            catch (JsonException) {
                continue;
            }

            if (string.IsNullOrEmpty(content)) continue;
            receivedContent = true;
            await writer.WriteAsync(content, cancellationToken).ConfigureAwait(false);
        }

        if (!state.Completed) throw new IOException("Qwen stream ended before the DONE marker.");
        if (!receivedContent) {
            await writer.WriteAsync("没有收到分析结果，请稍后重试。", cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class StreamReadState {
        public bool Completed { get; set; }
    }

    private static string? GetDeltaContent(JsonElement root) {
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0 || !choices[0].TryGetProperty("delta", out var delta) ||
            !delta.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.String) {
            return null;
        }

        return content.GetString();
    }

    private static HttpRequestMessage CreateRequest(string apiKey, string model, object travelData, bool stream) {
        var requestBody = new {
            model,
            messages = new[] {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = "请分析以下旅行数据并回答其中的 Question 字段：\n" + JsonSerializer.Serialize(travelData) }
            },
            stream
        };

        var request = new HttpRequestMessage(HttpMethod.Post, CompletionUrl) {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private async Task<string> GetErrorMessageAsync(HttpResponseMessage response, string model,
        CancellationToken cancellationToken = default) {
        string? errorCode = null;
        try {
            using var document = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object) {
                if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object) {
                    root = error;
                }
                if (root.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String) {
                    var value = code.GetString();
                    // Never display or log the raw provider response: it may contain request details.
                    if (value is { Length: > 0 and <= 80 } &&
                        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-')) {
                        errorCode = value;
                    }
                }
            }
        }
        catch (JsonException) {
            // A non-JSON gateway error still has a useful HTTP status.
        }
        catch (HttpRequestException) {
            // Keep the HTTP status if the error body cannot be read.
        }
        catch (IOException) {
            // Keep the HTTP status if the error body cannot be read.
        }

        _logger.LogWarning("AI request rejected: model {Model}, HTTP {StatusCode}, provider code {ErrorCode}",
            model, (int)response.StatusCode, errorCode ?? "unavailable");
        var detail = $"HTTP {(int)response.StatusCode}" + (errorCode is null ? "" : $"，{errorCode}");
        return response.StatusCode switch {
            HttpStatusCode.Unauthorized => $"千问鉴权失败（{detail}）。请重新粘贴完整的 API Key，并确认它未被重置、与接入地址匹配。",
            HttpStatusCode.Forbidden => $"千问平台拒绝访问 {model}（{detail}）。请检查业务空间的模型权限、服务开通状态及免费额度。",
            HttpStatusCode.TooManyRequests => $"千问请求受限（{detail}）。请稍后重试并检查平台用量。",
            _ => $"AI 服务请求失败（{detail}），请检查模型与平台设置后重试。"
        };
    }

    public void Dispose() => _httpClient.Dispose();
}
