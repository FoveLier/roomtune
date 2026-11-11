# RoomTune 工程约定

## 项目
- 项目名：声居 RoomTune · 数字房间声学校正系统
- PC：Visual Studio 2022 + .NET Framework 4.8 + WPF，独立可运行。
- 手机：HBuilderX + uni-app Vue3，Android APK；PC 与手机均可独立使用，通过同步码配合。
- 不使用微信小程序，不做 web-view、支付、手机号验证。

## 目录
- `desktop/`：Visual Studio 解决方案、Core 类库、WPF 桌面端、Core.Tests 控制台测试。
- `mobile/`：HBuilderX uni-app 工程。
- `docs/`：intent/spec/plan/DESIGN/JOURNEY/review/AI 披露/截图。
- `artifacts/`：本机构建产物（不入源码提交）。
- `scripts/`：构建、测试、打包辅助脚本。
- 运行时数据：PC 写入 `F:\RoomTuneData`；手机运行时写入应用 Storage；开发数据只在 F 盘。

## 约束
- UI 简洁清晰，不做无意义装饰、复杂渐变动效和 AI 次级标签。
- 声学计算必须确定性：驻波、Schroeder、RT60、SBIR、摆位、headroom、方案由规则引擎计算。
- AI 只做解释/报告，不得编造测量数据；无 API Key 时使用本地报告。
- PC 端不做实时音频 DSP、不控制 AVR；输出建议与参数卡。
- PC 工程使用 .NET Framework 4.8，避免新增需要联网安装的 NuGet 包；图表用 WPF 自绘。
- 手机端使用 uni-app Vue3；不引入微信小程序平台；图表用原生 CSS/Canvas 实现。
- 所有密钥只存 `F:\RoomTuneData\config.json` 或手机本地设置，不写源码/日志/提交历史。
- 每个阶段完成后更新 docs；最终 GitHub 发布放在 PC/手机都构建完成之后。
- 测试：PC 用 `desktop/tests` 控制台断言；手机用 Node 跑 `mobile/tests` 纯函数测试。
