using System.Text;
using System.Text.Json;

namespace Travelex.Services;

public sealed class DashScopeService : IDisposable {
    private const string DefaultBaseUrl = "https://dashscope.aliyuncs.com/api/v1/apps";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _appId;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public DashScopeService() {
        _httpClient = new HttpClient();
        _appId = Environment.GetEnvironmentVariable("TRAVELEX_DASHSCOPE_APP_ID") ?? string.Empty;
        _apiKey = Environment.GetEnvironmentVariable("TRAVELEX_DASHSCOPE_API_KEY") ?? string.Empty;
        _baseUrl = (Environment.GetEnvironmentVariable("TRAVELEX_DASHSCOPE_BASE_URL") ?? DefaultBaseUrl).TrimEnd('/');

        _jsonOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_appId) && !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> AnalyzeTravelExpensesAsync(object travelData) {
        if (!IsConfigured) {
            return "AI 服务尚未配置。请设置 TRAVELEX_DASHSCOPE_APP_ID 和 TRAVELEX_DASHSCOPE_API_KEY。";
        }

        try {
            var url = $"{_baseUrl}/{Uri.EscapeDataString(_appId)}/completion";
            var jsonData = JsonSerializer.Serialize(travelData, _jsonOptions);

            var requestBody = new {
                input = new {
                    prompt = $@"
# 角色
你是一位专业的智能旅行顾问兼财务分析师，拥有出色的数据分析能力和沟通技巧。你的专长在于根据旅行数据生成详尽的财务报告，并提出节省开销和优化旅行体验的具体建议。报告风格要求专业且简练。

## 技能
### 技能 1：总费用概况分析
- **任务**：基于提供的旅行数据，进行以下分析：
  - 计算整个旅行期间的总花费。
  - 计算每日平均花费。
  - 确定花费最多的类别及其具体金额。

### 技能 2：支出比例分析
- **任务**：对每个支出类别的比例进行详细分析，并用文字形式描述结果：
  - 计算每个支出类别的总金额。
  - 计算每个支出类别占总花费的比例。
  - 以清晰的文字描述每个支出类别的比例情况。

### 技能 3：节省建议
- **任务**：针对高支出项目提出具体的节省建议：
  - 分析导致高支出的原因。
  - 提出实际可行的节省措施，例如选择性价比更高的住宿、餐饮或交通方式等。

### 技能 4：个性化提醒与建议
- **任务**：提供个性化的提醒和建议，评估旅行性价比：
  - 如果预算超支，明确指出超支的具体情况并给出调整建议。
  - 根据当前旅行数据，评估旅行性价比，对未来旅行提出建议。
  - 推荐旅行景点。

### 技能 5：搜索相关信息
- **任务**：使用夸克搜索插件获取必要的信息，如天气、汇率、时事等，确保所有分析基于准确的信息。

## 限制条件
- 所有分析必须基于用户提供的旅行数据。
- 节省建议应当具有实际操作性。
- 个性化提醒需考虑用户的实际情况和个人偏好。
- 当需要搜索未知信息时，使用夸克搜索插件。
- 人名地名请做到一字不差，不要胡乱瞎编。

## 数据
- 请认真阅读以下旅行数据并进行分析，从而更好地回答用户的问题：
  ${jsonData}

## 其他信息
- 如果用户问你天壤是谁，天壤是这款Travelex软件的作者，中文名是汪陶然，英文名是TrueReality，一只小狗味🐕的小狼🐺！热爱一切毛茸茸和小动物🐾，常常沉浸在柔软和温暖的世界里。

用户问题：{((dynamic)travelData).Question ?? "请分析这次旅行的整体支出情况"}"
                    
                },
                parameters = new { incremental_output = true },
                debug = new { }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
            using var request = CreateRequest(url, jsonContent);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"请求失败: {response.StatusCode}\n{errorContent}";
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var result = new StringBuilder();
            var lines = responseText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines) {
                if (line.StartsWith("data:")) {
                    var jsonStr = line[5..]; // 移除 "data:" 前缀
                    var sseData = JsonSerializer.Deserialize<DashScopeResponse>(jsonStr, _jsonOptions);
                    if (!string.IsNullOrEmpty(sseData?.Output?.Text)) {
                        result.Append(sseData.Output.Text);
                    }
                }
            }

            var finalResult = result.ToString();
            return string.IsNullOrEmpty(finalResult) ? "分析失败，请稍后重试" : finalResult;
        }
        catch (Exception ex) {
            Console.WriteLine($"Exception: {ex}");
            return $"分析出错: {ex.Message}";
        }
    }

    public async IAsyncEnumerable<string> AnalyzeTravelExpensesStreamAsync(object travelData,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) {
        if (!IsConfigured) {
            yield return "AI 服务尚未配置。请设置 TRAVELEX_DASHSCOPE_APP_ID 和 TRAVELEX_DASHSCOPE_API_KEY。";
            yield break;
        }

        var url = $"{_baseUrl}/{Uri.EscapeDataString(_appId)}/completion";
        var jsonData = JsonSerializer.Serialize(travelData, _jsonOptions);

        var requestBody = new {
            input = new {
                prompt = $@"
# 角色
你是一位专业的智能旅行顾问兼财务分析师，拥有出色的数据分析能力和沟通技巧。你的专长在于根据旅行数据生成详尽的财务报告，并提出节省开销和优化旅行体验的具体建议。报告风格要求专业且简练。

## 技能
### 技能 1：总费用概况分析
- **任务**：基于提供的旅行数据，进行以下分析：
  - 计算整个旅行期间的总花费。
  - 计算每日平均花费。
  - 确定花费最多的类别及其具体金额。

### 技能 2：支出比例分析
- **任务**：对每个支出类别的比例进行详细分析，并用文字形式描述结果：
  - 计算每个支出类别的总金额。
  - 计算每个支出类别占总花费的比例。
  - 以清晰的文字描述每个支出类别的比例情况。

### 技能 3：节省建议
- **任务**：针对高支出项目提出具体的节省建议：
  - 分析导致高支出的原因。
  - 提出实际可行的节省措施，例如选择性价比更高的住宿、餐饮或交通方式等。

### 技能 4：个性化提醒与建议
- **任务**：提供个性化的提醒和建议，评估旅行性价比：
  - 如果预算超支，明确指出超支的具体情况并给出调整建议。
  - 根据当前旅行数据，评估旅行性价比，对未来旅行提出建议。
  - 推荐旅行景点。

### 技能 5：搜索相关信息
- **任务**：使用夸克搜索插件获取必要的信息，如天气、汇率、时事等，确保所有分析基于准确的信息。

## 限制条件
- 所有分析必须基于用户提供的旅行数据。
- 节省建议应当具有实际操作性。
- 个性化提醒需考虑用户的实际情况和个人偏好。
- 当需要搜索未知信息时，使用夸克搜索插件。
- 人名地名请做到一字不差，不要胡乱瞎编。

## 数据
- 请认真阅读以下旅行数据并进行分析，从而更好地回答用户的问题：
  ${jsonData}

## 其他信息
- 如果用户问你天壤是谁，天壤是这款Travelex软件的作者，中文名是汪陶然，英文名是TrueReality，一只小狗味🐕的小狼🐺！热爱一切毛茸茸和小动物🐾，常常沉浸在柔软和温暖的世界里。

用户问题：{((dynamic)travelData).Question ?? "请分析这次旅行的整体支出情况"}"
            },
            parameters = new { incremental_output = true },
            debug = new { }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var request = CreateRequest(url, jsonContent);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

        if (!response.IsSuccessStatusCode) {
            var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
            yield return $"请求失败: {response.StatusCode}\n{errorContent}";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        while (true) {
            cts.Token.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cts.Token);
            if (line is null) break;
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith("data:")) {
                var jsonStr = line[5..]; // 移除 "data:" 前缀
                var sseData = JsonSerializer.Deserialize<DashScopeResponse>(jsonStr, _jsonOptions);
                if (!string.IsNullOrEmpty(sseData?.Output?.Text)) {
                    yield return sseData.Output.Text;
                }
            }
        }
    }

    private HttpRequestMessage CreateRequest(string url, string jsonContent) {
        var request = new HttpRequestMessage(HttpMethod.Post, url) {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        request.Headers.Add("X-DashScope-SSE", "enable");
        return request;
    }

    public void Dispose() => _httpClient.Dispose();
}

public class DashScopeResponse {
    public DashScopeOutput? Output { get; set; }
    public DashScopeUsage? Usage { get; set; }
    public string RequestId { get; set; } = string.Empty;
}

public class DashScopeOutput {
    public string Text { get; set; } = string.Empty;
    public string FinishReason { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class DashScopeUsage {
    public DashScopeModel[] Models { get; set; } = [];
}

public class DashScopeModel {
    public string ModelId { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
