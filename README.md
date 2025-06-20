# Travelex - 您的智能旅行消费管家

[![.NET](https://github.com/actions/workflows/dotnet.yml/badge.svg)](https://github.com/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 📥 下载

您可以从我们的 [**GitHub Releases**](https://github.com/RealTrueReality/Travelex/releases) 页面下载最新的稳定版本。我们为 Windows 和 Android 提供了预编译的安装包。

[![Latest Release](https://img.shields.io/github/v/release/RealTrueReality/Travelex?display_name=tag&sort=semver)](https://github.com/RealTrueReality/Travelex/releases/latest)
Travelex 是一款基于 .NET MAUI Blazor Hybrid 构建的现代化、跨平台的旅行消费记录与分析应用。无论您是商务出行还是休闲度假，Travelex 都能帮助您轻松记录每一笔开销，并通过智能分析提供深刻的消费洞察。
## ✨ 核心理念

*   **统一体验**: 拒绝在不同设备间切换应用。Travelex 提供一套代码库，确保在 Windows, Android, iOS 和 macOS 上拥有一致、流畅的原生体验。
*   **智能洞察**: 超越简单的数字记录。我们集成了强大的 AI 模型，将您的消费数据转化为易于理解的摘要和建议，帮助您做出更明智的消费决策。
*   **现代与直观**: 借助最新的前端和后端技术，我们打造了一个既美观又易于上手的用户界面。
*   **开源与可定制**: 作为一个开源项目，我们欢迎社区的贡献，并鼓励开发者根据自身需求进行定制。

## ✨ 主要功能

*   **行程管理**: 创建和管理您的旅行计划，将消费记录与特定行程关联。
*   **消费记录**: 快速添加、编辑、删除各类消费，支持自定义分类。
*   **数据可视化**: 通过直观的图表（如条形图、饼图）分析您的消费构成和趋势。
*   **AI 智能分析**:
    *   **AI 摘要**: 与 AI 助手对话，获取基于您消费数据的智能摘要和建议。
    *   **流式分析**: 实时获取 AI 对您消费模式的深入分析。
*   **跨平台支持**: 一套代码库，同时支持 Windows, Android, iOS 和 macOS。
*   **现代化 UI**: 采用 Blazor 和 Tailwind CSS 构建，界面美观、响应迅速。
*   **明暗模式**: 支持手动和系统自动切换明暗主题，保护您的眼睛。
*   **地图集成**: 为行程目的地提供地图选择功能。

## 📸 应用截图

| 首页 | 行程详情 |
| :---: | :---: |
| ![图片](https://github.com/user-attachments/assets/4b01cbd2-b5fc-4942-8411-a2de54c8a92f)| ![图片](https://github.com/user-attachments/assets/d492313c-5c4e-4541-933f-8a45371d2d9c)|
| **图表分析** | **AI 智能摘要** |
| ![图片](https://github.com/user-attachments/assets/4b774dec-2d71-4943-9e2d-9ef7ff82444d)| ![图片](https://github.com/user-attachments/assets/2a5954ff-eda2-4c8c-9117-02923bf68c97)|

## 🛠️ 技术栈

*   **框架**: [.NET MAUI](https://dotnet.microsoft.com/apps/maui), [Blazor Hybrid](https://learn.microsoft.com/aspnet/core/blazor/hybrid)
*   **语言**: C#
*   **UI**: Razor Components, [Tailwind CSS](https://tailwindcss.com/), [Syncfusion Blazor](https://www.syncfusion.com/blazor-components)
*   **AI 服务**: 阿里云通义千问 (DashScope)
*   **数据库**: SQLite (本地存储)

## 🚀 快速开始

请确保您已安装最新的 [.NET SDK](https://dotnet.microsoft.com/download) 和 .NET MAUI 工作负载。

1.  **克隆仓库**
    ```bash
    git clone [https://github.com/your-username/Travelex.git](https://github.com/your-username/Travelex.git)
    cd Travelex
    ```

2.  **恢复依赖**
    ```bash
    dotnet restore
    ```

3.  **运行应用**

    *   **运行在 Windows 上:**
        ```bash
        dotnet build -t:Run -f net8.0-windows10.0.19041.0
        ```
    *   **运行在 Android 模拟器上:**
        ```bash
        dotnet build -t:Run -f net8.0-android
        ```
    *   **运行在 iOS 模拟器上 (需要 macOS 和 Xcode):**
        ```bash
        dotnet build -t:Run -f net8.0-ios
        ```

    > **注意**: 您可能需要在使用前在 `appsettings.json` 或相关服务配置中填入您自己的 AI 服务 API Key。

## 🤝 贡献

我们欢迎任何形式的贡献！如果您有好的想法或发现了 Bug，请随时提交 [Pull Request](https://github.com/RealTrueReality/Travelex/pulls) 或创建 [Issue](https://github.com/RealTrueReality/Travelex/issues)。

## 📄 许可证

本项目采用 [MIT](https://opensource.org/licenses/MIT) 许可证。
