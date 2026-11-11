# RoomTune JOURNEY

记录关键提示词、决策、验证与返工；不是完整聊天记录。

## 阶段 0：选题备案

**关键提示词**

```text
根据期末项目任务书，完成数字房间声学校正系统选题；
搜索 Dirac Live、Audyssey、REW/UMIK-1、房间声学与目标曲线资料；
先交选题一页纸，不做开发。
```

**产出**

- `F:\期末intent-李熹元-20241010113.md`（approved）
- `F:\期末项目-声学校正-调研笔记.md`
- 关键结论：现有系统多为硬件绑定/黑盒自动校准；缺少“摆位 + 装修 + 预算 + 参数卡 + AI 解释”的一体化。

## 阶段 1：需求修改与双端定位

**关键提示词**

```text
不做微信小程序；PC 用 Visual Studio 2022 写 WPF 独立应用；
手机用 HBuilderX 写 uni-app 并打包 APK；两端可独立使用，也可用同步码配合。
UI 简洁清晰，不做过多装饰/动效；所有数据写 F 盘。
```

**决策**

- PC：.NET Framework 4.8 WPF；不依赖联网 NuGet，图表/平面图自绘。
- 手机：HBuilderX uni-app Vue3；Android APK；本地 Storage；不接后端。
- 共享：项目 JSON 统一 PascalCase；同步码 Base64；导入生成新 ID。
- 不做的能力：实时 DSP、AVR 控制、云同步、微信小程序。

## 阶段 2：环境自检与 Skills

**关键提示词**

```text
检查 Visual Studio 2022、.NET、HBuilderX、Android 打包环境；
从 F:\skills 选择能帮助编写和检测代码的 skill，安装到项目文件夹。
```

**事实与处理**

- Visual Studio 2022 Community 17.14 安装在 `F:\VisualStudio2022`。
- `.NET SDK` 缺失，但 MSBuild 17.14 + .NET Framework 4.8 WPF 目标包可用；用 net48 项目验证构建成功。
- HBuilderX 5.34 已安装；官方 `publish web`/`pack` 需要 DCloud 登录和 AppID。
- 选装 Skills：superpowers（brainstorming、writing-plans、executing-plans、TDD、systematic-debugging、verification、requesting-code-review）；mattpocock（code-review、implement、diagnosing-bugs、tdd、prototype、codebase-design、to-spec、domain-modeling、grill-with-docs、grilling、research）。

## 阶段 3：PC Core

**关键提示词**

```text
先写确定性声学引擎和模型，再做 UI；
模态/RT60/Schroeder/SBIR/反射/headroom 都写单元断言；
参数卡必须标注估算/实测，不做深谷 boost。
```

**产出**

- `desktop/src/RoomTune.Core`：模型、材质、声学引擎、推荐、存储、同步、CSV、报告。
- 控制台测试 31 项通过。
- 关键 bug/修复：Windows PowerShell 输出编码与源文件 UTF-8 的差异只影响控制台显示；同步码用 Base64 JSON 跨端验证。

## 阶段 4：PC WPF

**关键提示词**

```text
用浅色工作台、左侧导航、白卡片；不要暗色炫技/渐变/动效；
房间页要能拖动主箱、低音炮和听音位；
分析页要能看到模态图/风险/SBIR/反射；
报告/测量/设置/同步码都要真实可用。
```

**验证**

- MSBuild Release 成功，窗口启动正常。
- 逐页截图：项目、房间、分析、方案、测量、报告、设置。
- 关键 UI 修复：表单标签“音箱总数”与低音炮计数语义对齐；布局/低音炮数量变化时重生成音箱位置；测量页示例 CSV 验证曲线与置信度。

## 阶段 5：手机端

**关键提示词**

```text
用 HBuilderX uni-app Vue3 写 6 页；UI 与 PC 同色系但适合单手；
手机可独立完成项目/房间/分析/方案/同步/设置；
声学核心与 PC 对齐；不要接微信小程序。
```

**产出**

- `mobile/`：6 页、浅色 tokens、tabBar、Canvas 平面图、CSS 模态图。
- Node 测试通过；HBuilderX H5 实际运行逐页检查。
- **发现的 bug**：手机 `MATERIALS` 没有 `AlphaAverage` 属性，导致 RT60 被钳到 0.15s；补上吸声系数平均函数后与 PC 的 1.10s 对齐。
- 关键截图：项目、房间、分析、方案、同步、设置。

## 阶段 6：跨端同步

**验证**

- PC `RoomProject` → `SyncCodec.Encode` → 手机 `decodeSync`：名称/尺寸/型号正确。
- 手机 `encodeSync` → PC `SyncCodec.Decode`：名称/尺寸/型号正确。
- 同步码约 1.5 KB，不含 AI Key/config；导入追加“（同步）”并生成新 ID。

## 阶段 7：APK 与 GitHub（进行中）

**关键提示词**

```text
HBuilderX 未登录 DCloud，先确认打包路线；
用户选择官方云打包：登录 HBuilderX、获取 AppID、pack Android。
PC/手机都完成后，再发布 GitHub 公开仓库；用户最后自行录屏。
```

**当前状态**

- HBuilderX 已登录（用户操作），已自动生成 AppID `__UNI__971A3F3`。
- 首次云打包缺少 `amazon-corretto`、`launcher`、`app-safe-pack` 插件；用户已在 HBuilderX 安装。
- 第二次打包因 DCloud 账号未绑定手机号 + 公共测试证书停用而失败；用户已绑定手机号。
- 第三次使用 Android 云端证书提交，项目编译成功；云端打包完成，APK 下载成功（14.8 MB，关闭统计）。
- GitHub 仓库：`https://github.com/FoveLier/roomtune`（公开）；Release `v1.0.0` 资产：PC zip、APK、code zip。
- PC Release 已构建，截图已生成。

## 阶段 8：审查与交付

- 双轴审查：Standards/Spec 两个只读子智能体，原文与分级见 `docs/review.md`。
- 修复后复审 1–14 项全部解决；PC 40 项断言、手机 Node 测试通过。
- APK 已下载：`F:\VisualStudio2022\program\RoomTune\artifacts\RoomTune-v1.0.0.apk`（14,764,918 bytes，SHA-256 `A6C1AB540FD1F2C9676692DBFC0E9B96CB765FFA3D23AF25A51D5BAB26C709DE`）。
- 待办：真实 Android 设备安装/启动验证（当前无设备）、用户自行录屏。





