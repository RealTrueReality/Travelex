using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Travelex.Services;

/// <summary>
/// First MAF integration step: one stateless agent over the platform's Chat Completions API.
/// Tools and persisted AgentSession state will be added after transport compatibility is verified.
/// </summary>
public sealed class MafTravelAgentService {
    private static readonly Uri Endpoint = new("https://maas.qianwenaiapi.com/compatible-mode/v1");
    private const string Instructions = "你是 Travelex 的旅行开支分析助手。只根据用户提供的数据分析总支出、分类占比、时间与地点趋势，并提出具体可行的节省建议。金额和日期必须准确；数据不足时明确说明，不要编造实时天气、汇率、景点信息或声称使用了搜索工具。请用简洁中文回答。";

    public async IAsyncEnumerable<string> AnalyzeStreamAsync(string apiKey, string model, object travelData,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        // Chat Completions is the compatibility surface already used by the existing client.
        // The key comes from MAUI SecureStorage at call time and is never put in source or preferences.
        var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions {
            Endpoint = Endpoint
        });
        AIAgent agent = client.GetChatClient(model).AsAIAgent(
            instructions: Instructions,
            name: "TravelexExpenseAnalyst");

        var prompt = "请分析以下旅行数据并回答其中的 Question 字段：\n" + JsonSerializer.Serialize(travelData);
        await foreach (var update in agent.RunStreamingAsync(prompt, cancellationToken: cancellationToken)
                           .ConfigureAwait(false)) {
            if (!string.IsNullOrEmpty(update.Text)) yield return update.Text;
        }
    }
}
