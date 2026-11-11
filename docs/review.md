# RoomTune 审查与修复记录

- 固定点：`4145ab7`（pre-review 双端实现与文档快照）
- 审查方式：`mattpocock/code-review`；Standards 轴与 Spec 轴由两个只读子智能体独立审查，不合并。
- 审查范围：当前快照全树（PC Core/WPF、mobile utils/pages/tests；不含 `.opencode/skills` 第三方源码）。
- 代码版本：修复前 `4145ab7`；修复后见 `review-fix` 提交。

## 一、Standards 轴原文

> **硬规范**
> 1. `mobile/pages/*.vue:3`（index/room/analysis/plan/settings/sync）、`App.vue:27`：每页英文 kicker（ROOM / SYSTEM…）— DESIGN §7 禁英文 eyebrow — 必须修复
> 2. `RoomView.xaml:62,100-110`、`room.vue:14,54,82-89`：听音位、灵敏度、声压、动态范围等输入无单位 — §6 全数值带单位 — 必须修复
> 3. `analysis.vue:21,144`：柱色按 <120Hz 分蓝橙而非模态类型；`ModeChartControl.cs:60-62`、`ResponseChartControl.cs:80`、`RoomPlanControl.cs:71-97` 色值硬编码，斜向用 #64748B≠token #94A3B8 — §2 — 必须修复
> 4. `sync.vue:11,21`：同页两个主按钮 — §1 主按钮唯一 — 必须修复
> 5. `App.vue:86`：次级按钮 70rpx，低于 76–80rpx — §2 — 必须修复
>
> **判断性异味**
> 6. `RoomProject.cs:61`＋`ProjectRepository.cs:39,48`：Normalize 每次读取都刷 UpdatedAt，按更新时间排序失真 — 副作用/Mysterious Name — 建议
> 7. `AppSession.cs:49-114`：四段“存配置→重载→重算→通知”逐字复制 — Duplicated Code / Shotgun Surgery — 建议
> 8. `RoomPlanControl.cs:43-46,119-122,149-152`：scale/offset 三处重算；`_dragTarget` 用 `"listener"` 字符串编码类型 — Duplicated Code / Primitive Obsession — 建议
> 9. `AcousticsEngine.cs:620-621`、`acoustics.js:331-332`：靠风险标题字符串计分 — Shotgun Surgery — 建议
> 10. `AcousticsEngine.cs:13`、`acoustics.js:3`、`ProjectRepository.cs:105`、`mobile/utils/format.js`：未用常量、恒被覆盖字段、整文件零引用 — Speculative Generality — 建议

## 二、Spec 轴原文

> - [必须修复][c] 模态只计算到 6 阶：spec 4.2“16–300 Hz”；AcousticsEngine.cs:78、acoustics.js:196；默认房间漏掉 226.8/292.8 Hz。
> - [必须修复][c] 布局生成不完整：7.1.4 仅生成 2 只天空音箱，2.0 默认仍生成低音炮；手机改低音炮数不重建音箱（spec 3.2/3.8“低音炮数量变化时…重生成”）；RoomProject.cs:199-227、acoustics.js:104-120/158、room.vue:74。
> - [必须修复][c] 每次读取/分析都会把 UpdatedAt 改成当前时间，导致更新时间和排序失真（spec 4.1）；RoomProject.cs:61、ProjectRepository.cs:39/48、acoustics.js:130、storage.js:32-33。
> - [必须修复][c] Storage/写盘错误被吞：手机损坏 JSON 被默认项目覆盖、写失败仍报已保存；PC 新建/删除/方案保存未捕获（spec 5“不覆盖原文件”/“不静默丢数据”）；storage.js:6-34、room.vue:188-189、ProjectsView.xaml.cs:71-99、PlanView.xaml.cs:37-50。
> - [必须修复][c] 旧 AI 文本跨项目残留、UseAi 勾选无效（spec 3.6“AI 只解释已算数据”）；ReportView.xaml.cs:23-26/81。
> - [必须修复][a] PC 分析页缺 RT60 区间；模态/SBIR/反射列表仅取 40/6/6、无风险空态（spec 3.3/5）；AnalysisView.xaml.cs:23-35、analysis.vue:32-44。
> - [建议][a] PC 装修清单缺面积、厚度（spec 3.4“面积、厚度”）；PlanView.xaml:44-51。
> - [建议][b] 手机空项目自动建默认项目，DESIGN 5“还没有项目”空态不出现；storage.js:24-34/75-99。
> - [建议][c] 同步码校验过宽：Base64(“{}”) 会导入默认项目（spec 5“同步码无效…不导入”）；无效 CSV/空码/JSON 测试也缺失（spec 7）；SyncCodec.cs:23-26、acoustics.js:484、Program.cs:19-24。

## 三、分级与处理

| # | 来源 | 问题 | 处理 |
| --- | --- | --- | --- |
| 1 | Standards | 手机英文 kicker | 接受；改为“手机工作台 / 房间/系统 / 声学/分析 / 校正/方案 / 同步/代码 / 设置/关于” |
| 2 | Standards | 数值输入缺单位 | 接受；PC/手机补 m、dB、Hz、% 单位 |
| 3 | Standards | 图表颜色/类型 | 接受；手机模态桶按“轴向→蓝、切向→橙、斜向→灰”；PC 斜向改为 `#94A3B8` |
| 4 | Standards | 同步页两个主按钮 | 接受；“生成当前项目同步码”为主按钮，“导入为新项目”改次级 |
| 5 | Standards | 手机次级按钮 70rpx | 接受；改为 76rpx |
| 6 | Standards/Spec | `UpdatedAt` 被读取/分析污染 | 接受；`Normalize` 不再写时间；仅在 Save 时更新；手机 saveProject 同理 |
| 7 | Spec | 模态只到 6 阶 | 接受；PC/手机阶数上限改 10，覆盖近 300 Hz |
| 8 | Spec | 2.0/7.1.4 布局不完整；手机低音炮数不重建 | 接受；2.0 清低音炮；7.1.4 增加左/右后天空；手机低音炮输入 blur 重建、布局变化重建 |
| 9 | Spec | Storage 写错/损坏被吞 | 接受；手机损坏键备份不覆盖；saveProject/新建/删除/清空在写失败时抛错，页面弹窗；PC 新建/删除/方案保存补 try/catch |
| 10 | Spec | AI 文本跨项目/UseAi 无效 | 接受；按项目 ID 清空 AI 文本；AI 点击检查 UseAi |
| 11 | Spec | PC 分析缺 RT60 区间/列表截断/无空态 | 接受；RT60 显示 90% 区间；列表全量；无风险显示“未发现明显高风险项” |
| 12 | Spec | 装修清单缺面积/厚度 | 接受；增加 `SizeText` 行 |
| 13 | Spec | 手机空项目自动建默认 | 接受；手机 Storage 空时显示空列表，用户主动新建；清空全部项目后为空 |
| 14 | Spec | 同步码/CSV 校验测试 | 接受；PC/mobile 增加无效同步测试；CSV 已有有效格式测试；空/无效提示已在 UI |
| 6/7/8/9/10 | Standards 建议 | AppSession 重复、RoomPlan 重复/字符串拖拽、风险标题计分、未用字段 | 记录为后续重构；本轮不扩大改动，避免引入 UI 回归 |

## 四、修复对照（代码）

| 问题 | 修复位置 | 做法 |
| --- | --- | --- |
| 更新时间污染 | `RoomProject.cs`、`ProjectRepository.cs`、`AcousticsEngine` 调用方；`mobile/utils/acoustics.js`、`storage.js` | Normalize 只做 clamp/补全；Save/saveProject 写入 UpdatedAt |
| 模态阶数 | `AcousticsEngine.CalculateModes`、`mobile/utils/acoustics.js` | orderLimit 10 |
| 布局不完整 | `SystemSpec.SeedLayout`、mobile `seedLayout` | 2.0 清低音炮；7.1.4 增加后天空；手机 sub blur 重建 |
| 项目管理/写盘错误 | `ProjectsView.xaml.cs`、`PlanView.xaml.cs`、`mobile/index`、`room`、`plan`、`settings`、`storage.js` | try/catch + 错误提示；Storage 损坏备份、不覆盖 |
| AI 残留 | `ReportView.xaml.cs` | `_aiProjectId`；UseAi 检查 |
| 分析空态/区间 | `AnalysisView.xaml.cs` | RT60 区间、全量列表、无风险提示 |
| 装修面积/厚度 | `PlanView.xaml` + `TreatmentItem.SizeText` | 显示面积/厚度 |
| 同步码过宽 | `SyncCodec.cs`、mobile `decodeSync`、测试 | 必含 Name/Room/System；无效 Base64/JSON 拒绝 |
| 手机空项目 | `mobile/utils/storage.js` | 不自动建默认；清空后为空 |
| 单位/英文 kicker/按钮 | mobile pages、App.vue、RoomView.xaml | 中文标签、单位、一个主按钮、76rpx |

## 五、验证

- PC Core：**40 项断言全通过**。
- 手机 Node：`mobile core tests passed`（137 模态、3 方案、同步码 1472 字符）。
- 双端同步：PC→手机、手机→PC 均实际解码成功；无效 `{}` 同步码被拒绝。
- 截图：`docs/screenshots/pc-*.png`、`mobile-*.png`；PC 重新构建启动，手机 H5 逐页运行。
- 已完成：HBuilderX Android 云打包（Android 云端证书，AppID `__UNI__971A3F3`），APK 下载成功（14,764,918 bytes，SHA-256 `A6C1AB540FD1F2C9676692DBFC0E9B96CB765FFA3D23AF25A51D5BAB26C709DE`）。GitHub 发布：`https://github.com/FoveLier/roomtune`；真实 Android 设备安装验证待完成（当前无设备）。

## 六、复审

复审范围：`git diff 4145ab7...HEAD`。子智能体逐项检查 1–14，初次结果：1–5、7、8、12、14 已解决；6、9、10、11、13 部分解决。随后补修并复验：

- 6 `UpdatedAt`：手机打开项目改为 `setCurrentProject(id)`，不再调用 `saveProject`；只有保存/新建/导入/删除改时间。PC Normalize 不写时间，Save 才写。
- 9 Storage 写错：新增 `getStorageError()` 并在项目页显示损坏提示；损坏原值备份到 `roomtune_projects_v1_corrupt_*`，不覆盖；saveProject/create/delete/clear 写失败抛错，页面弹窗；PC 项目/方案保存补 catch。
- 10 AI 串项目：请求前记录 projectId，返回后若当前项目已切换则丢弃；UseAi 未勾选时提示开启；切换项目清空旧 AI 文本。
- 11 手机分析：RT60 卡增加 0.90–1.30 s 区间；SBIR 列表不再只取前 5 条。
- 13 手机空项目：删除最后一个项目后保持空列表；清空全部项目也保持空列表，页面显示“还没有项目”。

**复审结论：1–14 项均已解决。** 剩余 Standards 判断性建议（AppSession 重复、RoomPlan scale 重复、风险标题计分、未用字段）记录为后续重构，不影响本轮验收。

### 新增验证

- PC Core：40 项断言全部通过。
- 手机：`mobile core tests passed`（137 模态、3 方案、同步码 1472 字符）。
- 双端同步：PC→手机、手机→PC 均成功；无效 `{}` 同步码被拒绝。
- PC 重新构建并启动成功；手机 H5 逐页运行。
- 已完成：HBuilderX 官方 Android 云打包（Android 云端证书）；APK 下载成功。GitHub 发布：`https://github.com/FoveLier/roomtune`；真实 Android 设备安装验证待完成（当前无设备）。





