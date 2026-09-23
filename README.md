# Travelex

Travelex 是一款基于 .NET MAUI Blazor Hybrid 的本地优先旅行消费管理应用。它把行程规划、消费记录、数据图表和 AI 财务分析串成一个完整流程，并以同一套代码支持 Windows、Android、iOS 和 Mac Catalyst。

## 主要功能

- 行程新增、编辑、删除、搜索、状态与分类管理
- 行程封面、日期、目的地和高德地图选点
- 消费记录、自定义消费类别与分类查看
- 全局及单次行程的消费构成、趋势和统计图表
- 基于阿里云 DashScope 应用的流式 AI 消费分析
- 本地注册、登录、头像及个人资料管理
- 明暗主题、系统主题跟随和首次启动引导

## 技术栈

- .NET 10、.NET MAUI、Blazor Hybrid
- Razor Components、Tailwind CSS
- Syncfusion Blazor Charts / Calendars / Popups
- sqlite-net-base、SQLitePCLRaw
- CommunityToolkit.Maui、CommunityToolkit.Mvvm
- 阿里云 DashScope、高德地图 JavaScript API

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

### DashScope

开发环境通过环境变量配置：

```powershell
$env:TRAVELEX_DASHSCOPE_APP_ID = "your-app-id"
$env:TRAVELEX_DASHSCOPE_API_KEY = "your-api-key"
```

macOS / Linux：

```bash
export TRAVELEX_DASHSCOPE_APP_ID="your-app-id"
export TRAVELEX_DASHSCOPE_API_KEY="your-api-key"
```

也可通过 `TRAVELEX_DASHSCOPE_BASE_URL` 覆盖默认 API 地址。未配置时，应用其他功能仍可使用，AI 页面会显示配置提示。

移动端正式发布时，不应把 DashScope Key 编译进客户端；建议由受控后端代理 AI 请求并在服务端保存凭据。

### 高德地图

复制示例配置：

```powershell
Copy-Item Travelex/wwwroot/js/runtime-config.example.js Travelex/wwwroot/js/runtime-config.local.js
```

然后填写自己的 Web Key 和安全密钥。`runtime-config.local.js` 已加入 `.gitignore`。发布前还应在高德控制台限制可使用该 Key 的平台或来源。

### Syncfusion

如需注册 Syncfusion License，请设置：

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

重新生成 Tailwind CSS：

```bash
cd Travelex
npm install
npm run css:build
```

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
