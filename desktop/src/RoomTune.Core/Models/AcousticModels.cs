using System.Collections.Generic;

namespace RoomTune.Core.Models
{
    public class RoomMode
    {
        public double FrequencyHz { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public int Order { get; set; }
        public double WavelengthM { get; set; }

        public override string ToString()
        {
            return string.Format("{0:0.0} Hz · {1} {2}", FrequencyHz, Type, Label);
        }
    }

    public class SbirIssue
    {
        public string SpeakerRole { get; set; }
        public string Surface { get; set; }
        public double DistanceM { get; set; }
        public double CancellationHz { get; set; }
        public string Severity { get; set; }
        public string Note { get; set; }
    }

    public class ReflectionPoint
    {
        public string SpeakerRole { get; set; }
        public string Surface { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DistanceM { get; set; }
        public string Note { get; set; }
    }

    public class RiskItem
    {
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Action { get; set; }

        public override string ToString()
        {
            return string.Format("{0} · {1}：{2}", Severity, Title, Action);
        }
    }

    public class AnalysisResult
    {
        public string ProjectId { get; set; }
        public double VolumeM3 { get; set; }
        public double SurfaceAreaM2 { get; set; }
        public double Rt60Seconds { get; set; }
        public double Rt60Low { get; set; }
        public double Rt60High { get; set; }
        public double SchroederHz { get; set; }
        public double HeadroomDb { get; set; }
        public double RecommendedMaxBoostDb { get; set; }
        public int ModeCount { get; set; }
        public List<RoomMode> Modes { get; set; }
        public List<SbirIssue> SbirIssues { get; set; }
        public List<ReflectionPoint> Reflections { get; set; }
        public List<RiskItem> Risks { get; set; }
        public string Confidence { get; set; }

        public AnalysisResult()
        {
            Modes = new List<RoomMode>();
            SbirIssues = new List<SbirIssue>();
            Reflections = new List<ReflectionPoint>();
            Risks = new List<RiskItem>();
            Confidence = "估算";
        }

        public string Rt60Band
        {
            get { return string.Format("{0:0.00} s（约 {1:0.00}–{2:0.00} s）", Rt60Seconds, Rt60Low, Rt60High); }
        }

        public string SchroederBand
        {
            get { return string.Format("{0:0} Hz", SchroederHz); }
        }
    }

    public class PlacementScheme
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Summary { get; set; }
        public int Score { get; set; }
        public List<string> Pros { get; set; }
        public List<string> Cons { get; set; }
        public List<string> Steps { get; set; }

        public PlacementScheme()
        {
            Name = "方案";
            Kind = "current";
            Summary = "";
            Pros = new List<string>();
            Cons = new List<string>();
            Steps = new List<string>();
        }

        public string ScoreText
        {
            get { return Score + " / 100"; }
        }

        public string ProsText
        {
            get { return Pros.Count == 0 ? "" : "优点：" + string.Join("；", Pros); }
        }

        public string ConsText
        {
            get { return Cons.Count == 0 ? "" : "取舍：" + string.Join("；", Cons); }
        }

        public string StepsText
        {
            get { return Steps.Count == 0 ? "" : string.Join("\r\n", Steps); }
        }
    }

    public class TreatmentItem
    {
        public int Priority { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public double AreaM2 { get; set; }
        public string Thickness { get; set; }
        public string FrequencyRange { get; set; }
        public string BudgetLevel { get; set; }
        public double BudgetCny { get; set; }
        public string Effect { get; set; }
        public string Note { get; set; }

        public string SizeText
        {
            get { return "面积 " + AreaText + " · " + Thickness; }
        }

        public string PriorityText
        {
            get { return "P" + Priority; }
        }

        public string AreaText
        {
            get { return AreaM2 <= 0 ? "—" : AreaM2.ToString("0.0") + " m²"; }
        }

        public string BudgetText
        {
            get { return BudgetCny <= 0 ? "免费" : "约 ¥" + BudgetCny.ToString("0"); }
        }
    }

    public class ParameterCard
    {
        public string Crossover { get; set; }
        public string Distances { get; set; }
        public string Delays { get; set; }
        public string Subwoofer { get; set; }
        public string TargetCurve { get; set; }
        public string EqHeadroom { get; set; }
        public List<string> PeqSuggestions { get; set; }
        public string Confidence { get; set; }

        public ParameterCard()
        {
            Crossover = "80 Hz";
            Distances = "";
            Delays = "";
            Subwoofer = "";
            TargetCurve = "Harman";
            EqHeadroom = "";
            PeqSuggestions = new List<string>();
            Confidence = "估算";
        }

        public string ToText()
        {
            var lines = new List<string>
            {
                "分频：" + Crossover,
                "距离：" + Distances,
                "延迟：" + Delays,
                "低音炮：" + Subwoofer,
                "目标曲线：" + TargetCurve,
                "EQ 余量：" + EqHeadroom,
                "PEQ 候选："
            };
            if (PeqSuggestions.Count == 0) lines.Add("  — 无（先实测再决定）");
            else foreach (var item in PeqSuggestions) lines.Add("  " + item);
            lines.Add("置信度：" + Confidence);
            return string.Join("\r\n", lines);
        }
    }

    public class RecommendationBundle
    {
        public AnalysisResult Analysis { get; set; }
        public List<PlacementScheme> Schemes { get; set; }
        public List<TreatmentItem> Treatments { get; set; }
        public ParameterCard Parameters { get; set; }
        public string Report { get; set; }

        public RecommendationBundle()
        {
            Schemes = new List<PlacementScheme>();
            Treatments = new List<TreatmentItem>();
            Parameters = new ParameterCard();
            Report = "";
        }
    }
}
