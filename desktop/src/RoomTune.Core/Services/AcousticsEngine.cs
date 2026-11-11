using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RoomTune.Core.Models;

namespace RoomTune.Core.Services
{
    public static class AcousticsEngine
    {
        public const double SpeedOfSound = 343.0;
        public const double TargetRt60 = 0.35;

        public static AnalysisResult Analyze(RoomProject project)
        {
            if (project == null) throw new ArgumentNullException("project");
            project.Normalize();
            var room = project.Room;
            var system = project.System;
            var result = new AnalysisResult
            {
                ProjectId = project.Id,
                VolumeM3 = room.VolumeM3,
                SurfaceAreaM2 = room.SurfaceAreaM2
            };

            double absorption = EstimateAbsorption(room);
            double rt60 = absorption <= 0 ? 0.8 : 0.161 * room.VolumeM3 / absorption;
            rt60 = Math.Max(0.15, Math.Min(1.8, rt60));
            result.Rt60Seconds = Math.Round(rt60, 2);
            result.Rt60Low = Math.Round(rt60 * 0.82, 2);
            result.Rt60High = Math.Round(rt60 * 1.18, 2);
            result.SchroederHz = Math.Round(2000.0 * Math.Sqrt(rt60 / Math.Max(1.0, room.VolumeM3)), 0);
            result.HeadroomDb = Math.Max(0, system.MaxSplDb - system.ListeningLevelDb);
            result.RecommendedMaxBoostDb = Math.Round(Math.Max(0, Math.Min(6, result.HeadroomDb - 6)), 1);

            result.Modes = CalculateModes(room.LengthM, room.WidthM, room.HeightM, 300);
            result.ModeCount = result.Modes.Count;
            result.SbirIssues = ComputeSbir(project);
            result.Reflections = ComputeReflections(project);
            result.Confidence = project.Measurement != null && project.Measurement.HasData
                ? "含 REW 测量参考"
                : "估算（未导入测量）";

            result.Risks = BuildRisks(project, result);
            return result;
        }

        public static double EstimateAbsorption(RoomSpec room)
        {
            var floor = MaterialLibrary.Get(room.FloorMaterial);
            var ceiling = MaterialLibrary.Get(room.CeilingMaterial);
            var wall = MaterialLibrary.Get(room.WallMaterial);
            var glass = MaterialLibrary.Get("玻璃窗");
            double floorArea = room.LengthM * room.WidthM;
            double ceilingArea = floorArea;
            double wallArea = 2.0 * (room.LengthM * room.HeightM + room.WidthM * room.HeightM);
            double windowRatio = Math.Max(0, Math.Min(0.9, room.WindowRatioPercent / 100.0));
            double wallAlpha = wall.AlphaAverage * (1 - windowRatio) + glass.AlphaAverage * windowRatio;
            double absorption = floorArea * floor.AlphaAverage
                                + ceilingArea * ceiling.AlphaAverage
                                + wallArea * wallAlpha
                                + MaterialLibrary.FurnitureAbsorption(room.FurnitureLevel);
            return Math.Max(0.5, absorption);
        }

        public static double EstimateRt60(RoomSpec room)
        {
            double absorption = EstimateAbsorption(room);
            return Math.Round(Math.Max(0.15, Math.Min(1.8, 0.161 * room.VolumeM3 / absorption)), 2);
        }

        public static List<RoomMode> CalculateModes(double length, double width, double height, double maxHz = 300)
        {
            var modes = new List<RoomMode>();
            var keys = new HashSet<string>();
            int orderLimit = 10;
            for (int nx = 0; nx <= orderLimit; nx++)
            {
                for (int ny = 0; ny <= orderLimit; ny++)
                {
                    for (int nz = 0; nz <= orderLimit; nz++)
                    {
                        if (nx == 0 && ny == 0 && nz == 0) continue;
                        double value = (SpeedOfSound / 2.0) * Math.Sqrt(
                            Square(nx / length) + Square(ny / width) + Square(nz / height));
                        if (value < 16 || value > maxHz) continue;
                        string type = nx > 0 && ny > 0 && nz > 0
                            ? "斜向"
                            : (nx > 0 && ny > 0) || (nx > 0 && nz > 0) || (ny > 0 && nz > 0)
                                ? "切向"
                                : "轴向";
                        string label = string.Format("({0},{1},{2})", nx, ny, nz);
                        string key = Math.Round(value, 0).ToString(CultureInfo.InvariantCulture);
                        if (keys.Contains(key)) continue;
                        keys.Add(key);
                        modes.Add(new RoomMode
                        {
                            FrequencyHz = Math.Round(value, 1),
                            Type = type,
                            Label = label,
                            Order = nx + ny + nz,
                            WavelengthM = Math.Round(SpeedOfSound / Math.Max(1, value), 2)
                        });
                    }
                }
            }
            return modes.OrderBy(item => item.FrequencyHz).ToList();
        }

        public static List<SbirIssue> ComputeSbir(RoomProject project)
        {
            var list = new List<SbirIssue>();
            var room = project.Room;
            foreach (var speaker in project.System.Speakers.Where(item => !item.IsSubwoofer))
            {
                AddSbir(list, speaker.Role, "前墙", speaker.Y);
                AddSbir(list, speaker.Role, "左侧墙", speaker.X);
                AddSbir(list, speaker.Role, "右侧墙", room.WidthM - speaker.X);
                AddSbir(list, speaker.Role, "地面", speaker.HeightM);
                AddSbir(list, speaker.Role, "天花", room.HeightM - speaker.HeightM);
            }
            foreach (var sub in project.System.Speakers.Where(item => item.IsSubwoofer))
            {
                AddSbir(list, sub.Role, "前墙", sub.Y);
                AddSbir(list, sub.Role, "左侧墙", sub.X);
                AddSbir(list, sub.Role, "右侧墙", room.WidthM - sub.X);
            }
            return list.OrderBy(item => item.CancellationHz).ToList();
        }

        private static void AddSbir(List<SbirIssue> list, string role, string surface, double distance)
        {
            if (distance <= 0.05 || distance > 4) return;
            double frequency = SpeedOfSound / (4.0 * distance);
            if (frequency < 20 || frequency > 300) return;
            string severity = distance < 0.45 ? "高" : distance < 1.0 ? "中" : "低";
            list.Add(new SbirIssue
            {
                SpeakerRole = role,
                Surface = surface,
                DistanceM = Math.Round(distance, 2),
                CancellationHz = Math.Round(frequency, 1),
                Severity = severity,
                Note = distance < 0.6 ? "离边界太近，优先挪位或做低频陷阱" : "用吸声/摆位减弱边界干涉"
            });
        }

        public static List<ReflectionPoint> ComputeReflections(RoomProject project)
        {
            var list = new List<ReflectionPoint>();
            var room = project.Room;
            double listenerX = room.ListenerX;
            double listenerY = room.ListenerY;
            foreach (var speaker in project.System.Speakers.Where(item => !item.IsSubwoofer))
            {
                AddReflection(list, speaker, "左侧墙", 0, speaker.X, speaker.Y, listenerX, listenerY, room.WidthM, room.LengthM);
                AddReflection(list, speaker, "右侧墙", room.WidthM, speaker.X, speaker.Y, listenerX, listenerY, room.WidthM, room.LengthM);
            }
            return list;
        }

        private static void AddReflection(List<ReflectionPoint> list, SpeakerPosition speaker, string surface,
            double wallX, double sx, double sy, double lx, double ly, double width, double length)
        {
            double mirrorX = wallX == 0 ? -sx : 2 * width - sx;
            double denominator = lx - mirrorX;
            if (Math.Abs(denominator) < 0.0001) return;
            double t = (wallX - mirrorX) / denominator;
            if (t <= 0 || t >= 1) return;
            double py = sy + t * (ly - sy);
            if (py <= 0 || py >= length) return;
            double first = Math.Sqrt(Square(wallX - sx) + Square(py - sy));
            double second = Math.Sqrt(Square(lx - wallX) + Square(ly - py));
            list.Add(new ReflectionPoint
            {
                SpeakerRole = speaker.Role,
                Surface = surface,
                X = Math.Round(wallX, 2),
                Y = Math.Round(py, 2),
                DistanceM = Math.Round(first + second, 2),
                Note = "镜面反射点，建议用 5cm 吸音板/厚窗帘处理"
            });
        }

        public static List<RiskItem> BuildRisks(RoomProject project, AnalysisResult result)
        {
            var risks = new List<RiskItem>();
            if (result.Rt60Seconds > 0.5)
            {
                risks.Add(new RiskItem
                {
                    Severity = "高",
                    Title = "混响时间偏长",
                    Detail = "估算 RT60 " + result.Rt60Band + "，硬墙/硬地面会放大反射。",
                    Action = "先铺厚地毯/挂厚窗帘，再在第一反射点加吸音板；四角优先低频陷阱。"
                });
            }
            else if (result.Rt60Seconds > 0.42)
            {
                risks.Add(new RiskItem
                {
                    Severity = "中",
                    Title = "混响略长",
                    Detail = "RT60 处于偏长区间，语言清晰度可能受影响。",
                    Action = "用软装增加中高频吸收，保持后墙适度扩散。"
                });
            }
            else if (result.Rt60Seconds < 0.22)
            {
                risks.Add(new RiskItem
                {
                    Severity = "低",
                    Title = "房间可能偏干",
                    Detail = "估算 RT60 很低，过度吸音会让声音发闷。",
                    Action = "减少满墙吸音，改用局部扩散和家具打破平行面。"
                });
            }

            var room = project.Room;
            double maxDim = Math.Max(room.LengthM, Math.Max(room.WidthM, room.HeightM));
            double minDim = Math.Min(room.LengthM, Math.Min(room.WidthM, room.HeightM));
            if (maxDim / Math.Max(0.1, minDim) > 2.3)
            {
                risks.Add(new RiskItem
                {
                    Severity = "中",
                    Title = "房间比例偏狭长",
                    Detail = "长宽高比例容易让轴向模态分布不均。",
                    Action = "听音位避开房间正中，音箱与后墙保留 0.6m 以上；低频优先多炮/低频陷阱。"
                });
            }

            var clustered = result.Modes.Where(item => item.FrequencyHz < 150).ToList();
            for (int i = 1; i < clustered.Count; i++)
            {
                if (clustered[i].FrequencyHz - clustered[i - 1].FrequencyHz < 2.5)
                {
                    risks.Add(new RiskItem
                    {
                        Severity = "中",
                        Title = "低频模态聚集",
                        Detail = string.Format("约 {0:0.0} Hz 与 {1:0.0} Hz 靠得很近，座位间低音差异可能明显。",
                            clustered[i - 1].FrequencyHz, clustered[i].FrequencyHz),
                        Action = "尝试 1/4 与 3/4 宽度多炮、改变听音位前后位置；用 REW 实测后再决定 EQ。"
                    });
                    break;
                }
            }

            if (result.SbirIssues.Any(item => item.Severity == "高"))
            {
                risks.Add(new RiskItem
                {
                    Severity = "高",
                    Title = "音箱离边界过近",
                    Detail = "存在离墙/离角小于 0.45m 的音箱，SBIR 可能造成低频凹陷。",
                    Action = "优先把主箱前移/内收，至少给前墙和侧墙留出 0.6m；低音炮另做摆位测试。"
                });
            }

            if (result.Reflections.Count > 0)
            {
                risks.Add(new RiskItem
                {
                    Severity = "中",
                    Title = "第一反射点需要处理",
                    Detail = "侧墙第一反射点会削弱声像与对白清晰度。",
                    Action = "在反射点放 5cm 吸音板或厚窗帘；不要整墙铺满，保留扩散。"
                });
            }

            var left = project.System.Speakers.FirstOrDefault(item => item.Role == "左主箱");
            var right = project.System.Speakers.FirstOrDefault(item => item.Role == "右主箱");
            if (left != null && right != null)
            {
                double symmetry = Math.Abs((room.WidthM - left.X - right.X));
                if (symmetry > 0.35)
                {
                    risks.Add(new RiskItem
                    {
                        Severity = "中",
                        Title = "左右摆位不对称",
                        Detail = "左右主箱到侧墙/听音位的距离差异较大，声像可能偏移。",
                        Action = "以听音位中心线为轴对称摆放，并让两只主箱 toe-in 角度一致。"
                    });
                }
                if (Math.Min(left.Y, right.Y) < 0.6)
                {
                    risks.Add(new RiskItem
                    {
                        Severity = "中",
                        Title = "主箱贴前墙",
                        Detail = "主箱离前墙太近，低频边界干涉与低频隆起风险上升。",
                        Action = "把主箱前移，至少留 0.6–1.0m；用 REW 比较前后频响。"
                    });
                }
            }

            if (result.HeadroomDb < 8)
            {
                risks.Add(new RiskItem
                {
                    Severity = "高",
                    Title = "系统动态余量不足",
                    Detail = "最大声压与听音音量只差 " + result.HeadroomDb.ToString("0.0") + " dB，EQ 提升空间很小。",
                    Action = "不要用 EQ 硬拉深谷；优先摆位/声装，目标音量留 10 dB 以上余量。"
                });
            }

            if (room.Shape != "矩形")
            {
                risks.Add(new RiskItem
                {
                    Severity = "低",
                    Title = "非矩形空间为近似计算",
                    Detail = "当前使用等效矩形估算，开放客餐厅/L 形的模态与反射会不同。",
                    Action = "把本结果作为方向性建议；关键参数用 REW 实测复核。"
                });
            }

            if (project.Measurement == null || !project.Measurement.HasData)
            {
                risks.Add(new RiskItem
                {
                    Severity = "低",
                    Title = "当前没有实测曲线",
                    Detail = "所有参数卡均为估算起点，不能替代 UMIK-1/REW 实测。",
                    Action = "在测量页导入 REW CSV；或在测量标签中查看精确/估算状态。"
                });
            }

            return risks.OrderBy(item => SeverityRank(item.Severity)).ToList();
        }

        public static RecommendationBundle BuildRecommendations(RoomProject project)
        {
            project.Normalize();
            var analysis = Analyze(project);
            var bundle = new RecommendationBundle { Analysis = analysis };

            var current = new PlacementScheme
            {
                Name = "现状微调",
                Kind = "current",
                Score = ScoreFromAnalysis(analysis),
                Summary = "保留现有家具与动线，只做不花钱的摆位/角度微调。",
                Steps = new List<string>
                {
                    "听音位保持在房间长度的 60–70%，避开正中线。",
                    "左右主箱到侧墙距离尽量对称，toe-in 朝听音位。",
                    "低音炮先做前后 30cm 的移动测试（听感/测量均可）。"
                }
            };
            current.Pros.Add("零成本、可逆");
            current.Pros.Add("不改变现有装修与动线");
            current.Cons.Add("不能解决房间本身的模态和混响问题");
            current.Cons.Add("效果受现有家具/墙位限制");
            bundle.Schemes.Add(current);

            var balancedProject = project.Clone();
            ApplyBalancedLayout(balancedProject);
            var balancedAnalysis = Analyze(balancedProject);
            var balanced = new PlacementScheme
            {
                Name = "对称等边摆位",
                Kind = "free",
                Score = ScoreFromAnalysis(balancedAnalysis),
                Summary = "把听音位移到 64% 长度，主箱与听音位形成近似等边三角形。",
                Steps = new List<string>
                {
                    "把听音位移到房间长度的约 64%，左右居中。",
                    "左/右主箱前移，离前墙 0.6–1.0m，左右到侧墙尽量对称。",
                    "主箱间距约等于到听音位的距离，toe-in 指向耳朵。",
                    "低音炮尝试前墙 1/4 与 3/4 宽度，选座位差异最小的位置。"
                }
            };
            balanced.Pros.Add("零成本、通常能改善声像和低频座位差异");
            balanced.Pros.Add("为后续声装/校准提供稳定基线");
            balanced.Cons.Add("可能受电视柜、门洞、沙发位置限制");
            balanced.Cons.Add("低音炮最终位置仍需实测");
            bundle.Schemes.Add(balanced);

            var treated = new PlacementScheme
            {
                Name = "摆位 + 声装",
                Kind = "treated",
                Score = Math.Min(98, balanced.Score + TreatmentScoreBonus(analysis)),
                Summary = "在对等边摆位基础上处理第一反射点、四角低频陷阱和软装。",
                Steps = new List<string>
                {
                    "先完成“对称等边摆位”，确认听音位与主箱角度。",
                    "左右侧墙第一反射点各放 1–2 块 5cm 吸音板，后墙适度扩散。",
                    "四角放 30–40cm 厚低频陷阱；地面铺厚地毯。",
                    "按预算先软装、后硬装，避免整墙吸音过干。"
                }
            };
            treated.Pros.Add("同时改善中高频清晰度与低频驻波");
            treated.Pros.Add("比单纯 EQ 更能处理深谷/时域问题");
            treated.Cons.Add("需要预算和施工");
            treated.Cons.Add("材料参数与施工质量会影响最终效果");
            bundle.Schemes.Add(treated);

            bundle.Treatments = BuildTreatments(project, analysis);
            bundle.Parameters = BuildParameterCard(project, analysis);
            bundle.Report = BuildReport(project, bundle);
            return bundle;
        }

        public static void ApplyBalancedLayout(RoomProject project)
        {
            project.Room.ListenerX = project.Room.WidthM / 2.0;
            project.Room.ListenerY = Math.Max(1.3, Math.Min(project.Room.LengthM - 0.8, project.Room.LengthM * 0.64));
            project.System.SeedLayout(project.Room);
        }

        public static List<TreatmentItem> BuildTreatments(RoomProject project, AnalysisResult analysis)
        {
            var items = new List<TreatmentItem>();
            double volume = analysis.VolumeM3;
            double target = project.System.TargetUse == "HiFi" ? 0.40 : 0.32;
            double currentAbsorption = EstimateAbsorption(project.Room);
            double currentRt = analysis.Rt60Seconds;
            double targetAbsorption = 0.161 * volume / Math.Max(0.2, target);
            double missing = Math.Max(0, targetAbsorption - currentAbsorption);
            double panelArea = 0.6 * 1.2;
            int panelCount = Math.Max(2, Math.Min(8, (int)Math.Ceiling(missing / Math.Max(0.15, 0.55) / 2.0)));
            int bassCount = 4;

            if (currentRt > target + 0.02)
            {
                items.Add(new TreatmentItem
                {
                    Priority = 1,
                    Title = "四角低频陷阱",
                    Location = "四个墙角，从地面到 1.2–1.8m",
                    AreaM2 = bassCount * 0.5,
                    Thickness = "30–40cm 厚，留空腔",
                    FrequencyRange = "40–200 Hz",
                    BudgetLevel = "中",
                    BudgetCny = bassCount * 450,
                    Effect = "压低低频驻波拖尾，改善轰头与座位差异",
                    Note = "角落是最有效的低频压力区；租房可用可移动角位陷阱。"
                });
            }

            if (analysis.Rt60Seconds > 0.38 || analysis.Reflections.Count > 0)
            {
                items.Add(new TreatmentItem
                {
                    Priority = 2,
                    Title = "第一反射点吸音板",
                    Location = "左右侧墙镜面反射点，必要时加天花",
                    AreaM2 = panelCount * panelArea,
                    Thickness = "5cm 多孔吸音，离墙 2–5cm",
                    FrequencyRange = "250 Hz–4 kHz",
                    BudgetLevel = "中",
                    BudgetCny = panelCount * 220,
                    Effect = "提升声像稳定度与对白清晰度",
                    Note = "先用手持镜法找点；不要整墙铺满，后墙可改扩散。"
                });
            }

            if (analysis.Rt60Seconds > 0.42)
            {
                items.Add(new TreatmentItem
                {
                    Priority = 2,
                    Title = "厚窗帘/厚地毯",
                    Location = "窗户与主听音区地面",
                    AreaM2 = Math.Round(project.Room.LengthM * project.Room.WidthM * 0.35, 1),
                    Thickness = "厚织物/短绒地毯",
                    FrequencyRange = "500 Hz–8 kHz",
                    BudgetLevel = "低",
                    BudgetCny = 600,
                    Effect = "降低中高频混响与硬地面反射",
                    Note = "软装优先，租房可逆；不要用薄布代替吸音板。"
                });
            }

            if (analysis.SchroederHz > 180)
            {
                items.Add(new TreatmentItem
                {
                    Priority = 3,
                    Title = "后墙扩散/书架",
                    Location = "听音位后墙或侧后方",
                    AreaM2 = 1.2,
                    Thickness = "2D 扩散板或满架书架",
                    FrequencyRange = "300 Hz–4 kHz",
                    BudgetLevel = "中",
                    BudgetCny = 800,
                    Effect = "打散后墙强反射，避免房间过干",
                    Note = "先吸第一反射点，再做扩散；小房间不要过度扩散。"
                });
            }

            if (items.Count == 0)
            {
                items.Add(new TreatmentItem
                {
                    Priority = 3,
                    Title = "暂不需要大改动",
                    Location = "保持现有软装与摆位",
                    AreaM2 = 0,
                    Thickness = "—",
                    FrequencyRange = "全频段",
                    BudgetLevel = "免费",
                    BudgetCny = 0,
                    Effect = "当前估算混响已接近目标",
                    Note = "用 REW 实测复核 20–300 Hz 再决定是否加吸音/扩散。"
                });
            }
            return items;
        }

        public static ParameterCard BuildParameterCard(RoomProject project, AnalysisResult analysis)
        {
            var card = new ParameterCard();
            var system = project.System;
            double crossover = Math.Max(40, Math.Min(120, Math.Max(80, system.LowFreqHz * 1.5)));
            card.Crossover = Math.Round(crossover / 5.0) * 5 + " Hz";

            var distances = new List<string>();
            var delays = new List<string>();
            foreach (var speaker in system.Speakers.Where(item => !item.IsSubwoofer))
            {
                double distance = Distance(speaker.X, speaker.Y, project.Room.ListenerX, project.Room.ListenerY);
                distances.Add(string.Format("{0} {1:0.0} m", speaker.Role, distance));
                delays.Add(string.Format("{0} {1:0.0} ms", speaker.Role, distance / SpeedOfSound * 1000.0));
            }
            card.Distances = string.Join("；", distances);
            card.Delays = string.Join("；", delays);
            card.Subwoofer = "先设 0° 相位，距离按实测；80 Hz 附近用 REW 比较正/反相，再决定相位与延迟。";
            card.TargetCurve = project.System.TargetCurve + "（" + project.System.TargetUse + "）";
            card.EqHeadroom = string.Format("最大声压 {0:0} dB - 听音音量 {1:0} dB = {2:0} dB；建议最大提升 {3:0.0} dB，深谷只做摆位/声装。",
                system.MaxSplDb, system.ListeningLevelDb, analysis.HeadroomDb, analysis.RecommendedMaxBoostDb);
            card.Confidence = analysis.Confidence;

            int count = 0;
            foreach (var mode in analysis.Modes.Where(item => item.FrequencyHz >= 40 && item.FrequencyHz <= 200).Take(5))
            {
                card.PeqSuggestions.Add(string.Format("{0:0.0} Hz：-3.0 dB，Q=8（仅切峰，先实测）", mode.FrequencyHz));
                count++;
                if (count >= 4) break;
            }
            if (count == 0)
            {
                card.PeqSuggestions.Add("未发现 40–200 Hz 的显著模态，先不要加 EQ。");
            }
            return card;
        }

        public static string BuildReport(RoomProject project, RecommendationBundle bundle)
        {
            var analysis = bundle.Analysis;
            var sb = new StringBuilder();
            sb.AppendLine("# " + project.Name + " · RoomTune 声学报告");
            sb.AppendLine();
            sb.AppendLine("> 本报告由确定性声学规则生成；没有实测时只作为房间诊断与摆位/装修起点，不替代 UMIK-1 + REW 或 Dirac/Audyssey。");
            sb.AppendLine();
            sb.AppendLine("## 房间与系统");
            sb.AppendLine(string.Format("- 房间：{0:0.0} × {1:0.0} × {2:0.0} m，体积 {3:0.0} m³，{4}",
                project.Room.LengthM, project.Room.WidthM, project.Room.HeightM, analysis.VolumeM3, project.Room.Shape));
            sb.AppendLine(string.Format("- 系统：{0}，主箱 {1}，低音炮 {2} 只；目标：{3} / {4}",
                project.System.Layout, project.System.SpeakerModel, project.System.SubwooferCount,
                project.System.TargetUse, project.System.TargetCurve));
            sb.AppendLine();
            sb.AppendLine("## 关键结果");
            sb.AppendLine("- 估算 RT60：" + analysis.Rt60Band);
            sb.AppendLine("- Schroeder 频率：" + analysis.SchroederBand);
            sb.AppendLine("- 模态：共 " + analysis.ModeCount + " 个（16–300 Hz 范围）；低频座位差异优先用摆位/多炮处理。");
            sb.AppendLine("- 动态余量：" + analysis.HeadroomDb.ToString("0.0") + " dB；建议最大 EQ 提升 " + analysis.RecommendedMaxBoostDb.ToString("0.0") + " dB。");
            sb.AppendLine("- 置信度：" + analysis.Confidence);
            sb.AppendLine();
            sb.AppendLine("## 风险");
            if (analysis.Risks.Count == 0) sb.AppendLine("- 未发现明显高风险项。");
            foreach (var risk in analysis.Risks)
            {
                sb.AppendLine(string.Format("- **{0} · {1}**：{2}  \n  处理：{3}", risk.Severity, risk.Title, risk.Detail, risk.Action));
            }
            sb.AppendLine();
            sb.AppendLine("## 摆位方案");
            foreach (var scheme in bundle.Schemes)
            {
                sb.AppendLine(string.Format("### {0}（{1}，{2}）", scheme.Name, scheme.Kind, scheme.ScoreText));
                sb.AppendLine(scheme.Summary);
                foreach (var step in scheme.Steps) sb.AppendLine("- " + step);
                sb.AppendLine();
            }
            sb.AppendLine("## 声学装修清单");
            foreach (var item in bundle.Treatments)
            {
                sb.AppendLine(string.Format("- {0} **{1}**：{2}，{3}，{4}，{5}，{6}  \n  {7}",
                    item.PriorityText, item.Title, item.Location, item.AreaText, item.Thickness,
                    item.FrequencyRange, item.BudgetText, item.Effect));
            }
            sb.AppendLine();
            sb.AppendLine("## 校准参数卡（估算起点）");
            sb.AppendLine("```text");
            sb.AppendLine(bundle.Parameters.ToText());
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## 下一步");
            sb.AppendLine("1. 先完成零成本摆位，听感/REW 对比。");
            sb.AppendLine("2. 处理第一反射点与四角低频陷阱，避免整墙吸音。");
            sb.AppendLine("3. 用 UMIK-1 + REW 在多个座位测量，再决定 PEQ/Dirac/Audyssey 参数。");
            return sb.ToString();
        }

        public static int ScoreFromAnalysis(AnalysisResult analysis)
        {
            int score = 100;
            if (analysis.Rt60Seconds > 0.6) score -= 24;
            else if (analysis.Rt60Seconds > 0.45) score -= 14;
            else if (analysis.Rt60Seconds < 0.2) score -= 8;
            if (analysis.SbirIssues.Any(item => item.Severity == "高")) score -= 16;
            if (analysis.SbirIssues.Any(item => item.Severity == "中")) score -= 6;
            if (analysis.Risks.Any(item => item.Title == "左右摆位不对称")) score -= 10;
            if (analysis.Risks.Any(item => item.Title == "低频模态聚集")) score -= 8;
            if (analysis.HeadroomDb < 8) score -= 10;
            return Math.Max(35, Math.Min(98, score));
        }

        private static int TreatmentScoreBonus(AnalysisResult analysis)
        {
            if (analysis.Rt60Seconds > 0.55) return 15;
            if (analysis.Rt60Seconds > 0.42) return 10;
            return 5;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Square(x2 - x1) + Square(y2 - y1));
        }

        private static double Square(double value)
        {
            return value * value;
        }

        private static int SeverityRank(string severity)
        {
            if (severity == "高") return 0;
            if (severity == "中") return 1;
            return 2;
        }
    }
}
