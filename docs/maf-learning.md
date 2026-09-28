# Travelex × Microsoft Agent Framework：第 1 步

本阶段只验证一件事：保持现有聊天界面与数据载荷不变，让同一个问题也能经由 MAF 的 `AIAgent` 流式回答。它还不是完整的“有记忆的旅行 Agent”。

## 当前调用链

原有通道：聊天页 → `QwenService` → 手工构造 Chat Completions 请求和解析 SSE → 千问平台。

MAF 预览通道：聊天页 → `QwenService` → `MafTravelAgentService` → `AIAgent` → OpenAI Chat Completions 兼容客户端 → 千问平台。

两条通道共用手机安全存储中的 API Key 和当前选择的模型。设置页的“试用 Microsoft Agent Framework 通道”默认关闭；开关只影响下一次聊天分析，不会自动重试或调用另一个模型。

## 为什么先用 Chat Completions

现有应用已经使用千问的 Chat Completions 兼容接口。MAF 也能以这种客户端创建 `AIAgent`，所以第一步只替换对话编排层，不同时更换 API 协议。不要把“OpenAI 兼容”理解成千问已实现 OpenAI Responses 的所有能力。

`AIAgent` 是运行对话的抽象；`OpenAIClient` 是访问兼容 API 的客户端；`MafTravelAgentService` 负责把二者连接起来。当前每次请求都创建一个新的 Agent，尚未传入 `AgentSession`，因此聊天页显示的历史消息不会成为模型的多轮上下文。

## 如何验证

1. 不需要 Key 即可先构建 Windows 和 Android 目标，确认依赖在 MAUI 项目中可用。
2. 待账号可用后，开启设置页的 MAF 预览，对同一条旅行支出问题分别使用原有通道和 MAF 通道；检查首字延迟、流式内容、取消与错误提示。
3. 若 MAF 通道出现问题，关闭开关即可回到原有通道。不要把真实 API Key、完整请求或旅行明细写入测试日志。

## 下一课

在兼容性通过后，先加入只读、限定行程范围的本地数据工具，令金额和分类统计由 C# 计算，再加入 `AgentSession` 和本地持久化。切换模型时应新建会话，避免把一个 Agent 的会话状态直接交给另一个配置。

参考：[MAF OpenAI 客户端](https://learn.microsoft.com/en-us/agent-framework/agents/providers/openai)、[会话](https://learn.microsoft.com/en-us/agent-framework/agents/conversations/session)、[千问 OpenAI 兼容接口](https://platform.qianwenai.com/docs/api-reference/toolkitframework/openai-compatible/overview)。
