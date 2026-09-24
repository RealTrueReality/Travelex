using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Travelex.Services;

/// <summary>
/// Personal-use Qwen client. A distributed app must use a backend proxy instead of a shared API key.
/// </summary>
public sealed class QwenService : IDisposable {
    private const string ApiKeyStorageKey = "Travelex.Qwen.ApiKey";
    private const string CompletionUrl = "https://maas.qianwenaiapi.com/compatible-mode/v1/chat/completions";
    private const string Model = "qwen3.7-plus";
    private const string SystemPrompt = "你是 Travelex 的旅行开支分析助手。只根据用户提供的数据分析总支出、分类占比、时间与地点趋势，并提出具体可行的节省建议。金额和日期必须准确；数据不足时明确说明，不要编造实时天气、汇率、景点信息或声称使用了搜索工具。请用简洁中文回答。";

    private readonly HttpClient _httpClient = new();

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

    public async Task<string> AnalyzeTravelExpensesAsync(object travelData) {
        var apiKey = await SecureStorage.Default.GetAsync(ApiKeyStorageKey);
        if (string.IsNullOrWhiteSpace(apiKey)) return "请先在 AI 助手中设置 API Key。";

        using var request = CreateRequest(apiKey, travelData, stream: false);
        try {
            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return GetErrorMessage(response.StatusCode);

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

        using var request = CreateRequest(apiKey, travelData, stream: true);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode) {
            yield return GetErrorMessage(response.StatusCode);
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var receivedContent = false;

        while (await reader.ReadLineAsync(cancellationToken) is { } line) {
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
            var data = line[5..].Trim();
            if (data == "[DONE]") break;
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
            yield return content;
        }

        if (!receivedContent) yield return "没有收到分析结果，请稍后重试。";
    }

    private static string? GetDeltaContent(JsonElement root) {
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0 || !choices[0].TryGetProperty("delta", out var delta) ||
            !delta.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.String) {
            return null;
        }

        return content.GetString();
    }

    private static HttpRequestMessage CreateRequest(string apiKey, object travelData, bool stream) {
        var requestBody = new {
            model = Model,
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

    private static string GetErrorMessage(HttpStatusCode statusCode) => statusCode switch {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "API Key 无效或没有模型访问权限，请检查后重试。",
        HttpStatusCode.TooManyRequests => "请求过于频繁或额度已用完，请稍后重试并检查平台用量。",
        _ => $"AI 服务请求失败（HTTP {(int)statusCode}），请稍后重试。"
    };

    public void Dispose() => _httpClient.Dispose();
}
