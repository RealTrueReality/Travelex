using System.Text;
using System.Text.Json;

namespace Travelex.Services;

public class DashScopeService
{
    private const string AppId = "73c75256f5fe48a599ecefa884770b6b";
    private const string ApiKey = "sk-636215b534d94ec7bf5a13dc0c28875c";
    private const string BaseUrl = "https://dashscope.aliyuncs.com/api/v1/apps";
    
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashScopeService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("X-DashScope-SSE", "enable");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<string> AnalyzeTravelExpensesAsync(object travelData)
    {
        try
        {
            var url = $"{BaseUrl}/{AppId}/completion";
            var jsonData = JsonSerializer.Serialize(travelData, _jsonOptions);
            
            var requestBody = new
            {
                input = new { prompt = $@"你是一位专业的旅行顾问和财务分析师。请根据以下数据回答用户的问题：

{jsonData}

用户问题：{((dynamic)travelData).Question ?? "请分析这次旅行的整体支出情况"}

要求：
1. 只回答用户提出的具体问题
2. 如果是分析整体情况，需要包含：总支出、平均每日支出、支出分布、合理性分析和建议
3. 回答要简洁明了，使用 Markdown 格式
4. 如果数据不足，请明确指出

请回答：" },
                parameters = new { incremental_output = true },
                debug = new { }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"请求失败: {response.StatusCode}\n{errorContent}";
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var result = new StringBuilder();
            var lines = responseText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("data:"))
                {
                    var jsonStr = line[5..]; // 移除 "data:" 前缀
                    var sseData = JsonSerializer.Deserialize<DashScopeResponse>(jsonStr, _jsonOptions);
                    if (!string.IsNullOrEmpty(sseData?.Output?.Text))
                    {
                        result.Append(sseData.Output.Text);
                    }
                }
            }

            var finalResult = result.ToString();
            return string.IsNullOrEmpty(finalResult) ? "分析失败，请稍后重试" : finalResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
            return $"分析出错: {ex.Message}";
        }
    }

    public async IAsyncEnumerable<string> AnalyzeTravelExpensesStreamAsync(object travelData, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/{AppId}/completion";
        var jsonData = JsonSerializer.Serialize(travelData, _jsonOptions);
        
        var requestBody = new
        {
            input = new { prompt = $@"你是一位专业的旅行顾问和财务分析师。请根据以下数据回答用户的问题：

{jsonData}

用户问题：{((dynamic)travelData).Question ?? "请分析这次旅行的整体支出情况"}

要求：
1. 只回答用户提出的具体问题
2. 如果是分析整体情况，需要包含：总支出、平均每日支出、支出分布、合理性分析和建议
3. 回答要简洁明了，使用 Markdown 格式
4. 如果数据不足，请明确指出

请回答：" },
            parameters = new { incremental_output = true },
            debug = new { }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var response = await _httpClient.PostAsync(url, content, cts.Token);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
            yield return $"请求失败: {response.StatusCode}\n{errorContent}";
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cts.Token.ThrowIfCancellationRequested();
            
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) continue;
            
            if (line.StartsWith("data:"))
            {
                var jsonStr = line[5..]; // 移除 "data:" 前缀
                var sseData = JsonSerializer.Deserialize<DashScopeResponse>(jsonStr, _jsonOptions);
                if (!string.IsNullOrEmpty(sseData?.Output?.Text))
                {
                    yield return sseData.Output.Text;
                }
            }
        }
    }
}

public class DashScopeResponse
{
    public DashScopeOutput Output { get; set; }
    public DashScopeUsage Usage { get; set; }
    public string RequestId { get; set; }
}

public class DashScopeOutput
{
    public string Text { get; set; }
    public string FinishReason { get; set; }
    public string SessionId { get; set; }
}

public class DashScopeUsage
{
    public DashScopeModel[] Models { get; set; }
}

public class DashScopeModel
{
    public string ModelId { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
