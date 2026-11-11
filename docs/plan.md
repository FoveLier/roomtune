# RoomTune 实施计划（Plan）

**Status: confirmed**

## 1. 技术路线

- **PC**：Visual Studio 2022（17.14，安装在 `F:\VisualStudio2022`）+ MSBuild + .NET Framework 4.8 + WPF；不使用需要联网还原的 NuGet 包，图表/平面图用 WPF 自绘。
- **手机**：HBuilderX 5.34 + uni-app Vue3；Android APK；原生 CSS/Canvas 图表；不接微信小程序。
- **共享数据格式**：PascalCase JSON；同步码 = Base64(UTF8(JSON))；PC/手机双向解码。
- **数据目录**：PC 运行时 `F:\RoomTuneData`；手机运行时 App Storage；源码、文档、构建产物在 F 盘项目目录。

## 2. 项目结构

```text
F:\VisualStudio2022\program\RoomTune\
├── .opencode/skills/        选定并安装的代码编写/测试/审查 Skills
├── desktop/
│   ├── RoomTune.sln
│   ├── src/RoomTune.Core/   模型、声学引擎、存储、同步、CSV、报告
│   ├── src/RoomTune.Desktop/WPF、页面、2D 平面图、图表
│   └── tests/RoomTune.Core.Tests/ 控制台断言测试
├── mobile/                  HBuilderX uni-app Vue3 工程
│   ├── pages/{index,room,analysis,plan,sync,settings}
│   ├── utils/{acoustics,storage,format}.js
│   └── tests/acoustics.test.mjs
├── docs/
├── scripts/                 构建/截图/打包辅助
└── artifacts/               本机构建产物（不提交源码）
```

## 3. 实施任务

### Task 0：环境与 Skills

- [x] 定位 Visual Studio 2022：`F:\VisualStudio2022`，MSBuild 17.14；确认 .NET Framework 4.8 WPF 目标包可用。
- [x] 验证 WPF `net48` 最小项目可构建。
- [x] 从 F:\skills 选择并安装代码编写/测试/审查技能到项目 `.opencode/skills`：
  - superpowers：brainstorming、writing-plans、executing-plans、test-driven-development、systematic-debugging、verification-before-completion、requesting-code-review。
  - mattpocock：code-review、implement、diagnosing-bugs、tdd、prototype、codebase-design、to-spec、domain-modeling、grill-with-docs、grilling、research。
- [x] 记录 HBuilderX 未登录时无法 `publish web`/云打包；后由用户登录 DCloud 并配置 AppID（APK 打包前完成）。

### Task 1：PC Core

- [x] `RoomProject` / `RoomSpec` / `SystemSpec` / `SpeakerPosition` / `MeasurementData` / `SavedScheme` / `AppConfig`。
- [x] `MaterialLibrary`：吸声系数、家具吸收、材质下拉。
- [x] `AcousticsEngine`：模态、RT60、Schroeder、SBIR、第一反射、headroom、风险。
- [x] `BuildRecommendations`：三个方案、装修清单、参数卡、Markdown 报告。
- [x] `ProjectRepository`：F 盘 projects/exports/config/logs；JSON 容错。
- [x] `SyncCodec`：Base64 JSON 跨端。
- [x] `RewCsvParser`：REW CSV。
- [x] `ReportExporter`：Markdown/HTML。
- [x] 控制台测试 31 项通过。

### Task 2：PC WPF

- [x] App 资源：浅色工作台、卡片、按钮、表格控件样式；无动画。
- [x] MainWindow 侧边导航 + 数据目录 + 同步码入口。
- [x] 项目页：新建/打开/删除/导入同步码/打开数据目录。
- [x] 房间与系统页：表单、材质/布局/目标 picker、2D 可拖动平面图、保存/重置/对称摆位。
- [x] 声学分析：汇总卡、模态图/列表、风险、SBIR、第一反射。
- [x] 校正方案：三方案卡、装修清单、参数卡、应用/保存/复制/同步。
- [x] 测量：REW CSV 导入/清除、对数频率曲线、置信度。
- [x] 报告：本地报告、Markdown/HTML 导出、复制、AI 可选。
- [x] 设置：AI Base/Model/Key、数据目录。
- [x] 构建 Release exe，启动并逐页截图。

### Task 3：手机端

- [x] uni-app 项目骨架、浅色 tokens、tabBar、6 个页面。
- [x] `utils/acoustics.js`：与 PC 对齐的模态/RT60/Schroeder/SBIR/反射/推荐/参数卡/同步码。
- [x] `utils/storage.js`：项目列表、当前项目、导入、清空。
- [x] 项目/房间/分析/方案/同步/设置页；Canvas 平面图。
- [x] Node 纯函数测试通过；H5 实际运行逐页检查。
- [x] HBuilderX 官方 Android 云打包（Android 云端证书）→ 输出 APK。

### Task 4：跨端与验收

- [x] PC→手机同步码：PC `SyncCodec.Encode` → 手机 `decodeSync` 成功。
- [x] 手机→PC：手机 `encodeSync` → PC `SyncCodec.Decode` 成功。
- [x] PC Core 测试、手机 JS 测试。
- [x] PC/手机截图（项目、房间、分析、方案、测量、报告、设置、同步）。
- [ ] 在真实 Android 设备/模拟器安装 APK 并启动验证（若环境可用）。
- [x] GitHub 发布：公开仓库 + 文档 + 截图 + Release（PC zip、APK、code zip）。
- [ ] 用户录屏（用户自行完成）。

### Task 5：审查与修复

- [ ] 用 `mattpocock/code-review` 双轴（Standards/Spec）审查 PC Core、WPF、移动端与同步逻辑。
- [ ] 修复必须项；复审修复差异；把原文/分级/对照写入 `docs/review.md`。
- [ ] 更新 `docs/ai-disclosure.md`、`docs/JOURNEY.md`、README。

## 4. 提交阶段（建议）

```text
base        项目骨架、Skills、AGENTS
docs         intent/spec/plan/DESIGN
pc-core      声学引擎、存储、同步、CSV、测试
pc-ui        WPF 页面与自绘图表
mobile       手机端页面与核心
integration  双端同步、截图、构建脚本
review       审查记录与修复
release      APK/PC zip/GitHub 发布
```

## 5. 验证命令

```powershell
# PC Core 测试
& 'F:\VisualStudio2022\MSBuild\Current\Bin\MSBuild.exe' `
  'F:\VisualStudio2022\program\RoomTune\desktop\tests\RoomTune.Core.Tests\RoomTune.Core.Tests.csproj' `
  /t:Rebuild /p:Configuration=Release
& 'F:\VisualStudio2022\program\RoomTune\desktop\tests\RoomTune.Core.Tests\bin\Release\RoomTune.Core.Tests.exe'

# PC Release
& 'F:\VisualStudio2022\MSBuild\Current\Bin\MSBuild.exe' `
  'F:\VisualStudio2022\program\RoomTune\desktop\src\RoomTune.Desktop\RoomTune.Desktop.csproj' `
  /t:Rebuild /p:Configuration=Release

# 手机核心测试
cd 'F:\VisualStudio2022\program\RoomTune\mobile'
node tests/acoustics.test.mjs

# H5 预览（HBuilderX）
& 'F:\HBuilderX\cli.exe' launch web --project 'F:\VisualStudio2022\program\RoomTune\mobile'

# 官方 Android 云打包（需登录 DCloud + manifest appid）
& 'F:\HBuilderX\cli.exe' pack --project 'F:\VisualStudio2022\program\RoomTune\mobile' `
  --platform android --android.packagename com.roomtune.app --android.androidpacktype 1
```

## 6. 风险与处理

| 风险 | 处理 |
| --- | --- |
| HBuilderX 未登录/无 AppID | 用户登录 DCloud 并获取免费 AppID；登录后再 pack；不把账号信息写入源码 |
| H5 直接 build 受 HBuilderX CLI 限制 | 以 HBuilderX 实际运行验证 UI；APK 用官方云打包；保留 `launch web` 日志 |
| 手机/PC 同步字段不一致 | 统一 PascalCase JSON；双向编解码已实际验证 |
| 数值格式/国际化 | PC 使用 CurrentCulture 与 InvariantCulture 双尝试；手机 normalizeNumber |
| 房间非矩形 | 使用等效矩形估算，风险卡明确“需实测” |
| 无测量时过度承诺 | 参数卡/报告统一标注“估算起点”，不输出“已校正”结论 |
| APK 云打包耗时/排队 | 提前登录/AppID；保留构建日志；失败时记录服务端错误 |

