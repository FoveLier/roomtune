# RoomTune 声居 · 数字房间声学校正系统

> 李熹元 · 20241010113 · 期末项目
>
> GitHub：https://github.com/FoveLier/roomtune
>
> 手机 APK：`artifacts/RoomTune-v1.0.0.apk`（HBuilderX Android 云端证书构建）

RoomTune 把家庭房间的真实尺寸、材质、音箱/听音位和 REW 测量，转成可执行的声学建议：先摆位，再软装/声装，最后才做 EQ/校准参数。它不是实时 DSP，也不直接控制 AVR/Dirac；它负责“先做什么、为什么、参数是什么”。

## 双端

| 端 | 技术 | 产物 |
| --- | --- | --- |
| PC | Visual Studio 2022 + WPF + .NET Framework 4.8 | `RoomTune.Desktop.exe`，独立运行 |
| 手机 | HBuilderX + uni-app Vue3 | Android APK，独立运行；与 PC 通过同步码配合 |

PC 与手机都包含：房间档案、音箱/动态范围、声学分析、摆位/声装方案、参数卡。PC 额外支持 2D 拖拽、REW CSV 导入、报告导出与 AI 解释；手机支持项目列表、Canvas 平面图和同步码。

## 目录

```text
RoomTune/
├── desktop/
│   ├── RoomTune.sln
│   ├── src/RoomTune.Core/       声学模型/引擎/存储/同步/CSV/报告
│   ├── src/RoomTune.Desktop/    WPF 应用与自绘图表
│   └── tests/RoomTune.Core.Tests/ 控制台断言测试
├── mobile/                      HBuilderX uni-app 工程
│   ├── pages/                   项目/房间/分析/方案/同步/设置
│   ├── utils/                   声学核心/Storage/格式化
│   └── tests/                   Node 纯函数测试
├── docs/                        intent/spec/plan/DESIGN/JOURNEY/review/AI 披露/截图
├── scripts/                     构建、截图、打包辅助
├── .opencode/skills/            本次安装并使用的 Skills
├── AGENTS.md
└── README.md
```

## PC 运行

环境：Windows 10/11，.NET Framework 4.8，Visual Studio 2022（或 MSBuild 17）。

1. 打开 `desktop/RoomTune.sln`（或 `desktop/src/RoomTune.Desktop/RoomTune.Desktop.csproj`）。
2. 编译 `Release`；运行 `desktop/src/RoomTune.Desktop/bin/Release/RoomTune.Desktop.exe`。
3. 项目数据在 `F:\RoomTuneData\projects`；报告导出到 `F:\RoomTuneData\exports`；配置在 `F:\RoomTuneData\config.json`。

命令行构建：

```powershell
& 'F:\VisualStudio2022\MSBuild\Current\Bin\MSBuild.exe' `
  'F:\VisualStudio2022\program\RoomTune\desktop\src\RoomTune.Desktop\RoomTune.Desktop.csproj' `
  /t:Rebuild /p:Configuration=Release
```

## PC 测试

```powershell
& 'F:\VisualStudio2022\MSBuild\Current\Bin\MSBuild.exe' `
  'F:\VisualStudio2022\program\RoomTune\desktop\tests\RoomTune.Core.Tests\RoomTune.Core.Tests.csproj' `
  /t:Rebuild /p:Configuration=Release
& 'F:\VisualStudio2022\program\RoomTune\desktop\tests\RoomTune.Core.Tests\bin\Release\RoomTune.Core.Tests.exe'
```

## 手机运行 / 调试

1. HBuilderX 打开 `mobile/`。
2. 运行到 Web（Chrome）可做开发期预览；运行到 Android 需 HBuilderX 登录 DCloud 并配置 manifest AppID。
3. 手机数据存 App Storage；不需要登录/后端。

已生成 APK：`artifacts/RoomTune-v1.0.0.apk`（14.8 MB，Android 云端证书；HBuilderX 云打包，关闭 uni统计）。安装前请允许未知来源；真机启动验证建议在 Android 8+ 上进行。

手机核心测试：

```powershell
cd 'F:\VisualStudio2022\program\RoomTune\mobile'
node tests/acoustics.test.mjs
```

## 同步码

PC 侧边栏“同步码”或方案页“导出同步码” → 复制 → 手机“同步”页粘贴 → “导入为新项目”。

反向：手机“同步”页“生成当前项目同步码” → 复制 → PC 同步码窗口粘贴 → “导入为新项目”。

同步码是项目 JSON 的 Base64 文本，不包含 AI Key；导入会生成新 ID，不覆盖原项目。

## 声学规则摘要

- 矩形模态 `f = (c/2)sqrt((nx/L)^2+(ny/W)^2+(nz/H)^2)`，16–300 Hz。
- Sabine `RT60 = 0.161 V / A`，展示 ±18% 区间。
- Schroeder `fs = 2000 sqrt(RT60/V)`。
- SBIR `f = c/(4d)`，优先处理离墙 < 0.6 m 的主箱。
- 动态余量 `MaxSpl - ListeningLevel`，最大 EQ 提升受 headroom 限制。
- 先摆位/软装/低频陷阱，后 EQ；不做深谷硬 boost。
- 参数卡中的分频/延迟/相位/PEQ 是估算起点，精确值需 UMIK-1 + REW 实测。

## AI（可选）

PC 设置页可填写 OpenAI-compatible API（Base URL / model / Key），在报告页点击“AI 解释（可选）”。Key 只保存在 `F:\RoomTuneData\config.json`；不写入源码、日志或提交历史。没有 Key 时本地规则报告完整可用，AI 只解释已经算出的数据，不产生测量值。

## 数据与隐私

- PC 项目/配置/报告只写 F 盘 `F:\RoomTuneData`。
- 手机数据只写 App 本机 Storage；不联网同步。
- 不接入微信小程序、web-view、支付、手机号验证。
- 不采集网易云 Cookie、GitHub Token 等敏感信息。

## 文档

- `docs/intent.md`：选题 intent（approved）
- `docs/spec.md`：功能/算法/验收（approved）
- `docs/plan.md`：实施计划（confirmed）
- `docs/DESIGN.md`：双端 UI 约束
- `docs/JOURNEY.md`：关键提示词与过程
- `docs/GIT-LOG.md`：本地提交记录
- `docs/DEMO.md`：录屏脚本
- `docs/review.md`：双轴审查与修复
- `docs/ai-disclosure.md`：AI 工具/模型/Skills 披露
- `docs/screenshots/`：PC/手机运行截图
- `docs/DEMO.md`：4–6 分钟录屏脚本（本人录制）

## 演示录屏

由本人最后录制 4–6 分钟：完整操作演示（项目 → 房间 → 分析 → 方案 → 测量 → 报告 + 手机端同步），特色说明与已知边界。


