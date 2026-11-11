# RoomTune AI 使用披露

## 1. 基本信息

- 姓名/学号：李熹元 · 20241010113
- AI 工具：OpenCode（桌面端）
- 模型：DeepSeek V4.1 Flash（provider：opencode-go / model：deepseek-v4.1-flash）
- 主要 IDE：Visual Studio 2022（PC）、HBuilderX 5.34（手机）
- Skills 来源：`F:\skills\superpowers-7.0.2`、`F:\skills\skills-1.3.1`
- 项目目录：`F:\VisualStudio2022\program\RoomTune`

## 2. 各阶段使用

| 阶段 | 工具/模型 | Skills | 用途 | 人工核对 |
| --- | --- | --- | --- | --- |
| 选题与调研 | OpenCode / DeepSeek | `mattpocock/research`（调研方法） | Dirac/Audyssey/REW/房间声学资料整理，形成 intent 与调研笔记 | 本人确认题目与边界 |
| 需求与规格 | OpenCode / DeepSeek | `to-spec`、`domain-modeling` | 将 intent 拆成双端 spec、数据模型、验收标准 | 本人确认不做微信/云同步 |
| 计划与设计 | OpenCode / DeepSeek | `writing-plans`、`prototype`、`codebase-design` | 任务分解、UI 约束、双端设计 tokens | 本人确认简单浅色 UI |
| PC Core | OpenCode / DeepSeek | `test-driven-development`、`systematic-debugging` | C# 模型、声学公式、推荐、存储、同步、CSV、报告 | 本人运行 31 项断言 |
| PC WPF | OpenCode / DeepSeek | `implement`、`codebase-design` | 左侧导航、表单、自绘平面图/图表、报告/设置 | 本人逐页截图检查 |
| 手机端 | OpenCode / DeepSeek | `implement`、`tdd` | uni-app 6 页、Canvas、JS 声学核心、Storage | 本人逐页 H5 检查 + Node 测试 |
| 跨端同步 | OpenCode / DeepSeek | `verification-before-completion` | Base64 JSON 双向编解码验证 | 本人用 PC/手机实际编解码 |
| 代码审查 | OpenCode / DeepSeek + 两个只读子智能体 | `code-review`（Standards/Spec 双轴） | 审查声学代码、WPF/移动端与同步逻辑；输出分级 | 本人确认修复项 |
| 打包/发布 | OpenCode / DeepSeek | `verification-before-completion` | HBuilderX 云打包、PC Release、GitHub 发布 | 本人登录 DCloud/GitHub；发布前确认 |

## 3. Skills 选择理由

选择原则：只安装能帮助“写代码、测代码、查代码”的小而可组合技能，不使用会接管流程的脚手架。

- `superpowers`：brainstorming/writing-plans/executing-plans 负责流程；TDD/systematic-debugging/verification 负责质量；requesting-code-review 负责审查入口。
- `mattpocock/skills`：code-review 做双轴审查；implement/diagnosing-bugs/tdd 用于日常修改；to-spec/domain-modeling 用于文档；prototype/codebase-design 用于 UI/模块边界。
- 安装位置：项目 `.opencode/skills/`，与源码同一项目文件夹，符合作业要求。

## 4. AI 使用边界

- 声学数值（模态、RT60、Schroeder、SBIR、headroom、预算/面积）由确定性代码计算；AI 不产生测量数据。
- AI 报告只解释已算数据；没有 API Key 时本地报告完整可用。
- 密钥只存本机配置，不写入源码、日志、提交历史；提交前做 secret scan。
- 本人对最终代码、UI、文档、打包产物负责；AI 输出均经实际运行/测试/截图核对。
- 演示录屏由本人最后录制，不由 AI 代录。

## 5. 未使用/未做的事项

- 未使用个人网易云 Cookie、音乐平台 API。
- 未使用微信小程序、web-view、支付、手机号验证。
- 未使用实时音频 DSP、AVR/Dirac 控制或云同步。
- 未在代码/文档中保存 GitHub Token 或 DCloud 密码；账号登录由本人完成。
