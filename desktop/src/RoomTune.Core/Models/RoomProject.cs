using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace RoomTune.Core.Models
{
    public class RoomProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public RoomSpec Room { get; set; }
        public SystemSpec System { get; set; }
        public MeasurementData Measurement { get; set; }
        public List<SavedScheme> SavedSchemes { get; set; }

        public RoomProject()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = "未命名房间";
            Notes = "";
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UpdatedAt = CreatedAt;
            Room = new RoomSpec();
            System = new SystemSpec();
            Measurement = new MeasurementData();
            SavedSchemes = new List<SavedScheme>();
        }

        public static RoomProject CreateNew(string name = "客厅影院")
        {
            var project = new RoomProject { Name = name };
            project.Room.ListenerX = project.Room.WidthM / 2.0;
            project.Room.ListenerY = project.Room.LengthM * 0.64;
            project.System.SeedLayout(project.Room);
            return project;
        }

        public RoomProject Clone()
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(this);
            var copy = serializer.Deserialize<RoomProject>(json);
            copy.Normalize();
            return copy;
        }

        public void Normalize()
        {
            if (Room == null) Room = new RoomSpec();
            if (System == null) System = new SystemSpec();
            if (Measurement == null) Measurement = new MeasurementData();
            if (SavedSchemes == null) SavedSchemes = new List<SavedScheme>();
            if (string.IsNullOrWhiteSpace(Id)) Id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(Name)) Name = "未命名房间";
            Room.Normalize();
            System.Normalize(Room);
            if (string.IsNullOrWhiteSpace(CreatedAt)) CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class RoomSpec
    {
        public double LengthM { get; set; }
        public double WidthM { get; set; }
        public double HeightM { get; set; }
        public string Shape { get; set; }
        public string FloorMaterial { get; set; }
        public string WallMaterial { get; set; }
        public string CeilingMaterial { get; set; }
        public double WindowRatioPercent { get; set; }
        public string FurnitureLevel { get; set; }
        public double ListenerX { get; set; }
        public double ListenerY { get; set; }

        public RoomSpec()
        {
            LengthM = 5.4;
            WidthM = 3.8;
            HeightM = 2.7;
            Shape = "矩形";
            FloorMaterial = "木地板";
            WallMaterial = "乳胶漆墙面";
            CeilingMaterial = "石膏板吊顶";
            WindowRatioPercent = 20;
            FurnitureLevel = "中等";
            ListenerX = WidthM / 2.0;
            ListenerY = LengthM * 0.64;
        }

        public void Normalize()
        {
            LengthM = Clamp(LengthM, 2.0, 20.0);
            WidthM = Clamp(WidthM, 2.0, 20.0);
            HeightM = Clamp(HeightM, 2.0, 6.0);
            WindowRatioPercent = Clamp(WindowRatioPercent, 0, 90);
            if (string.IsNullOrWhiteSpace(Shape)) Shape = "矩形";
            if (string.IsNullOrWhiteSpace(FloorMaterial)) FloorMaterial = "木地板";
            if (string.IsNullOrWhiteSpace(WallMaterial)) WallMaterial = "乳胶漆墙面";
            if (string.IsNullOrWhiteSpace(CeilingMaterial)) CeilingMaterial = "石膏板吊顶";
            if (string.IsNullOrWhiteSpace(FurnitureLevel)) FurnitureLevel = "中等";
            ListenerX = Clamp(ListenerX, 0.4, WidthM - 0.4);
            ListenerY = Clamp(ListenerY, 0.4, LengthM - 0.4);
        }

        public double VolumeM3 { get { return LengthM * WidthM * HeightM; } }
        public double FloorAreaM2 { get { return LengthM * WidthM; } }
        public double SurfaceAreaM2
        {
            get
            {
                return 2 * (LengthM * WidthM + LengthM * HeightM + WidthM * HeightM);
            }
        }

        private static double Clamp(double value, double min, double max)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }
    }

    public class SystemSpec
    {
        public string Layout { get; set; }
        public int SpeakerCount { get; set; }
        public int SubwooferCount { get; set; }
        public string SpeakerModel { get; set; }
        public double LowFreqHz { get; set; }
        public double HighFreqHz { get; set; }
        public double SensitivityDb { get; set; }
        public double MaxSplDb { get; set; }
        public double DynamicRangeDb { get; set; }
        public double ListeningLevelDb { get; set; }
        public string TargetUse { get; set; }
        public string TargetCurve { get; set; }
        public List<SpeakerPosition> Speakers { get; set; }

        public SystemSpec()
        {
            Layout = "2.1";
            SpeakerCount = 2;
            SubwooferCount = 1;
            SpeakerModel = "通用书架箱";
            LowFreqHz = 45;
            HighFreqHz = 20000;
            SensitivityDb = 87;
            MaxSplDb = 105;
            DynamicRangeDb = 95;
            ListeningLevelDb = 75;
            TargetUse = "电影";
            TargetCurve = "Harman";
            Speakers = new List<SpeakerPosition>();
        }

        public void Normalize(RoomSpec room)
        {
            Layout = string.IsNullOrWhiteSpace(Layout) ? "2.1" : Layout;
            SubwooferCount = Math.Max(0, Math.Min(4, SubwooferCount));
            SpeakerCount = Math.Max(2, Math.Min(16, SpeakerCount));
            LowFreqHz = Math.Max(20, Math.Min(500, LowFreqHz));
            HighFreqHz = Math.Max(2000, Math.Min(40000, HighFreqHz));
            SensitivityDb = Math.Max(70, Math.Min(110, SensitivityDb));
            MaxSplDb = Math.Max(85, Math.Min(140, MaxSplDb));
            DynamicRangeDb = Math.Max(60, Math.Min(130, DynamicRangeDb));
            ListeningLevelDb = Math.Max(50, Math.Min(110, ListeningLevelDb));
            if (string.IsNullOrWhiteSpace(TargetCurve)) TargetCurve = "Harman";
            if (string.IsNullOrWhiteSpace(TargetUse)) TargetUse = "电影";
            if (Speakers == null) Speakers = new List<SpeakerPosition>();
            if (Speakers.Count == 0)
            {
                SeedLayout(room);
            }
            foreach (var speaker in Speakers)
            {
                speaker.X = Math.Max(0, Math.Min(room.WidthM, speaker.X));
                speaker.Y = Math.Max(0, Math.Min(room.LengthM, speaker.Y));
                speaker.Angle = Math.Max(-90, Math.Min(90, speaker.Angle));
            }
        }

        public void SeedLayout(RoomSpec room)
        {
            Speakers = new List<SpeakerPosition>();
            if (Layout == "2.0")
            {
                SubwooferCount = 0;
            }
            double listenerX = room.ListenerX;
            double listenerY = room.ListenerY;
            double frontY = Math.Max(0.55, room.LengthM * 0.16);
            double half = Math.Max(0.55, Math.Min(room.WidthM * 0.30, 1.35));
            double leftX = Math.Max(0.45, listenerX - half);
            double rightX = Math.Min(room.WidthM - 0.45, listenerX + half);
            double angle = Math.Atan2(half, Math.Max(0.8, listenerY - frontY)) * 180.0 / Math.PI;

            Speakers.Add(new SpeakerPosition { Role = "左主箱", X = leftX, Y = frontY, Angle = angle, HeightM = 1.1 });
            Speakers.Add(new SpeakerPosition { Role = "右主箱", X = rightX, Y = frontY, Angle = -angle, HeightM = 1.1 });

            if (Layout == "5.1" || Layout == "5.1.2" || Layout == "7.1.4")
            {
                Speakers.Add(new SpeakerPosition { Role = "中置", X = listenerX, Y = Math.Max(0.45, frontY - 0.15), Angle = 0, HeightM = 1.1 });
                Speakers.Add(new SpeakerPosition { Role = "左环绕", X = Math.Max(0.35, room.WidthM * 0.14), Y = listenerY, Angle = 35, HeightM = 1.2 });
                Speakers.Add(new SpeakerPosition { Role = "右环绕", X = Math.Min(room.WidthM - 0.35, room.WidthM * 0.86), Y = listenerY, Angle = -35, HeightM = 1.2 });
            }
            if (Layout == "5.1.2" || Layout == "7.1.4")
            {
                Speakers.Add(new SpeakerPosition { Role = "左天空", X = leftX, Y = listenerY - 0.8, Angle = 20, HeightM = 2.2 });
                Speakers.Add(new SpeakerPosition { Role = "右天空", X = rightX, Y = listenerY - 0.8, Angle = -20, HeightM = 2.2 });
            }
            if (Layout == "7.1.4")
            {
                Speakers.Add(new SpeakerPosition { Role = "左后环绕", X = Math.Max(0.3, room.WidthM * 0.12), Y = Math.Min(room.LengthM - 0.4, listenerY + 0.9), Angle = 55, HeightM = 1.2 });
                Speakers.Add(new SpeakerPosition { Role = "右后环绕", X = Math.Min(room.WidthM - 0.3, room.WidthM * 0.88), Y = Math.Min(room.LengthM - 0.4, listenerY + 0.9), Angle = -55, HeightM = 1.2 });
                Speakers.Add(new SpeakerPosition { Role = "左后天空", X = leftX, Y = Math.Min(room.LengthM - 0.4, listenerY + 0.7), Angle = 30, HeightM = 2.2 });
                Speakers.Add(new SpeakerPosition { Role = "右后天空", X = rightX, Y = Math.Min(room.LengthM - 0.4, listenerY + 0.7), Angle = -30, HeightM = 2.2 });
            }
            for (int i = 0; i < SubwooferCount; i++)
            {
                double subX = SubwooferCount == 1 ? listenerX : (i == 0 ? room.WidthM * 0.28 : room.WidthM * 0.72);
                Speakers.Add(new SpeakerPosition
                {
                    Role = "低音炮" + (i + 1),
                    X = subX,
                    Y = Math.Max(0.25, room.LengthM * 0.10),
                    Angle = 0,
                    HeightM = 0.35,
                    IsSubwoofer = true
                });
            }
            SpeakerCount = Speakers.Count;
        }
    }

    public class SpeakerPosition
    {
        public string Role { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double HeightM { get; set; }
        public bool IsSubwoofer { get; set; }

        public SpeakerPosition()
        {
            Role = "音箱";
            HeightM = 1.1;
            Angle = 0;
        }
    }

    public class FrequencyPoint
    {
        public double FrequencyHz { get; set; }
        public double SplDb { get; set; }
    }

    public class MeasurementData
    {
        public string Source { get; set; }
        public string ImportedAt { get; set; }
        public List<FrequencyPoint> Points { get; set; }

        public MeasurementData()
        {
            Source = "";
            ImportedAt = "";
            Points = new List<FrequencyPoint>();
        }

        public bool HasData { get { return Points != null && Points.Count > 0; } }
    }

    public class SavedScheme
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Summary { get; set; }
        public string ParameterCard { get; set; }
        public string CreatedAt { get; set; }

        public SavedScheme()
        {
            Name = "方案";
            Kind = "current";
            Summary = "";
            ParameterCard = "";
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class AppConfig
    {
        public string AiBaseUrl { get; set; }
        public string AiModel { get; set; }
        public string AiApiKey { get; set; }
        public bool UseAi { get; set; }
        public string LastProjectId { get; set; }
        public string DataFolder { get; set; }

        public AppConfig()
        {
            AiBaseUrl = "https://api.deepseek.com/v1";
            AiModel = "deepseek-chat";
            AiApiKey = "";
            UseAi = false;
            LastProjectId = "";
            DataFolder = @"F:\RoomTuneData";
        }
    }
}
