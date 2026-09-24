# Travelex

Travelex 是一款基于 .NET MAUI Blazor Hybrid 的本地优先旅行消费管理应用。它把行程规划、消费记录、数据图表和 AI 财务分析串成一个完整流程，并以同一套代码支持 Windows、Android、iOS 和 Mac Catalyst。

## 主要功能

- 行程新增、编辑、删除、搜索、状态与分类管理
- 行程封面、日期、目的地和高德地图选点
- 消费记录、自定义消费类别与分类查看
- 全局及单次行程的消费构成、趋势和统计图表
- 基于千问 AI 模型的流式旅行消费分析
- 本地注册、登录、头像及个人资料管理
- 明暗主题、系统主题跟随和首次启动引导

## 技术栈

- .NET 10、.NET MAUI、Blazor Hybrid
- Razor Components、Tailwind CSS
- Syncfusion Blazor Charts / Calendars / Popups
- sqlite-net-base、SQLitePCLRaw
- CommunityToolkit.Maui、CommunityToolkit.Mvvm
- 千问 AI 平台、高德地图 JavaScript API

## 架构

```text
App / AppShell / MainPage
        │
        └── BlazorWebView
                │
                ├── Razor Pages & Components
                │       │
                │       └── Travel / Expense / Auth / AI Services
                │                       │
                │                       └── DatabaseContext → SQLite
                │
                └── JS Interop → 地图、主题、剪贴板
```

项目主要采用组件化 Blazor UI 和服务层结构。ViewModel 当前仅用于少量原生 UI 状态，因此它不是严格的全局 MVVM 架构。

本地数据库位于 MAUI 的 `FileSystem.AppDataDirectory/Travelex.db`。首次启动时会创建表并写入示例分类、行程和消费数据。

## 环境要求

- .NET SDK 10.0，具体策略见 `global.json`
- 对应平台的 .NET MAUI workload
- Android 构建需要 Android SDK 和兼容的 JDK
- iOS / Mac Catalyst 构建需要 macOS 和 Xcode
- Node.js，仅在重新生成 Tailwind CSS 时需要

安装 MAUI workload：

```bash
dotnet workload install maui
```

## 本地配置

仓库不会保存 API Key、许可证或签名密码。历史版本曾包含开发凭据；从旧版本迁移时，请先在对应平台轮换这些凭据。

### 千问 AI

AI 助手使用千问 AI 平台的 OpenAI 兼容 `chat/completions` 接口（默认模型 `qwen3.7-plus`），不再需要旧版 DashScope 应用的 APP ID。个人在手机上使用时，打开「AI助手」或单次旅行 AI 分析页，展开「千问 API Key」，输入自己的 `sk-ws-` Key。Key 仅保存在该设备的系统安全存储中，不写入源码、安装包或 Windows 用户环境变量；旅行分析数据会发送至千问 AI 平台。可在同一处移除或更换 Key。

此方式仅适合个人自用。移动客户端仍可能被设备持有者分析，**公开分发或多人使用时不要在客户端保存长期 API Key**，应改由受控后端代理请求、保护凭据并限制用量。[接口接入文档](https://platform.qianwenai.com/docs/developer-guides/getting-started/first-api-call) · [密钥安全建议](https://platform.qianwenai.com/docs/developer-guides/administration/api-keys)

### 高德地图

复制示例配置：

```powershell
Copy-Item Travelex/wwwroot/js/runtime-config.example.js Travelex/wwwroot/js/runtime-config.local.js
```

然后填写自己的 Web Key 和安全密钥。`runtime-config.local.js` 已加入 `.gitignore`。发布前还应在高德控制台限制可使用该 Key 的平台或来源。

### Syncfusion

在本机创建 `Travelex/syncfusion-license.local.txt`，内容为一整行 Syncfusion License Key。该文件已被 Git 忽略，构建时会嵌入应用，并在 Syncfusion 控件初始化前注册。重新构建、安装应用后生效。

Codemagic 发布构建请配置加密环境变量 `TRAVELEX_SYNCFUSION_LICENSE_KEY`；构建脚本会把它写入同名的本地文件。直接从终端运行项目时也可以设置该环境变量作为备用方式：

```powershell
$env:TRAVELEX_SYNCFUSION_LICENSE_KEY = "your-license-key"
```

### Windows 签名

签名文件和密码不要写入项目文件，可在发布命令或 CI Secret 中传入：

```powershell
dotnet publish Travelex/Travelex.csproj `
  -f net10.0-windows10.0.19041.0 `
  -c Release `
  -p:PackageCertificateKeyFile="C:\secure\Travelex.pfx" `
  -p:PackageCertificatePassword="$env:TRAVELEX_CERT_PASSWORD"
```

## 构建与运行

还原依赖：

```bash
dotnet restore Travelex.sln
```

构建当前平台：

```bash
dotnet build Travelex.sln
```

Windows：

```powershell
dotnet build Travelex/Travelex.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Android：

```bash
dotnet build Travelex/Travelex.csproj -t:Run -f net10.0-android
```

安装前端构建依赖并重新生成 Tailwind CSS（在仓库根目录执行）：

```bash
npm ci
npm run css:build
```

`codemagic.yaml` 也会在打包前执行这两步。修改 Razor 中的 Tailwind 类名、`Travelex/tailwind.config.js` 或 `Travelex/wwwroot/css/app.css` 后，请重新生成并提交 `app.min.css`。

页面样式优先使用语义化颜色（`bg-canvas`、`bg-surface`、`text-content`、`text-content-muted`、`border-outline`、`bg-brand`）。明暗主题的实际色值统一定义在 `Travelex/wwwroot/css/app.css`；旧的 `*-light` / `*-dark` 类仍可兼容使用，新增页面请使用语义类。常用页面容器、图标按钮、主次按钮、卡片、输入框和空状态分别使用 `app-page`、`icon-button`、`btn-primary` / `btn-secondary`、`surface-card`、`form-control`、`empty-state`。页面标题栏通过 `AppPageShell` 复用。

## 数据与安全说明

- 用户密码使用带随机盐的 PBKDF2-SHA256 哈希保存，不会写入登录会话。
- 旧版本产生的明文密码会在用户首次成功登录后自动升级为哈希格式。
- 当前数据完全保存在本机，没有云同步或跨设备共享。
- 本地登录用于区分应用访问状态，目前行程和消费表尚未按用户 ID 隔离。
- 客户端本地数据库不等同于安全保险库；敏感数据应使用 MAUI SecureStorage 或平台密钥链。

## 项目结构

```text
Travelex/
├── Components/       Blazor 页面、布局和可复用控件
├── Data/             SQLite 连接与通用数据操作
├── Entities/         Travel、Expense、User 等持久化实体
├── Models/           表单与结果模型
├── Pages/            原生 MAUI 页面
├── Platforms/        Android、iOS、Mac Catalyst、Windows 配置
├── Services/         认证、行程、消费、AI、主题和定位服务
├── Utils/            MAUI 与 Blazor 互操作
├── ViewModels/       原生 UI 状态
└── wwwroot/          CSS、JavaScript、字体和图片
```

## 当前限制

- 尚无自动化测试项目
- 尚无数据库 schema 迁移框架
- 尚无云同步、共享行程、OCR 和多货币换算
- 部分较大的 Razor 页面仍适合进一步拆分

## CI

`codemagic.yaml` 提供 iOS 与 Mac Catalyst 的 Codemagic 构建流程。证书、Provisioning Profile 和外部服务凭据应全部通过 CI 的加密变量或 Secret 管理。

## License

项目代码按 MIT License 使用。第三方组件、字体、图片和外部服务分别受其自身许可协议约束。
