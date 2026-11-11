using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomTune.Core.Models
{
    public class AcousticMaterial
    {
        public string Name { get; set; }
        public double Alpha500 { get; set; }
        public double Alpha1000 { get; set; }
        public string Description { get; set; }

        public double AlphaAverage
        {
            get { return (Alpha500 + Alpha1000) / 2.0; }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class MaterialLibrary
    {
        public static readonly List<AcousticMaterial> Materials = new List<AcousticMaterial>
        {
            new AcousticMaterial { Name = "乳胶漆墙面", Alpha500 = 0.03, Alpha1000 = 0.04, Description = "硬墙，反射强" },
            new AcousticMaterial { Name = "壁纸墙面", Alpha500 = 0.05, Alpha1000 = 0.06, Description = "薄壁纸" },
            new AcousticMaterial { Name = "木饰面墙", Alpha500 = 0.08, Alpha1000 = 0.10, Description = "木饰面/护墙板" },
            new AcousticMaterial { Name = "玻璃窗", Alpha500 = 0.03, Alpha1000 = 0.02, Description = "大面积玻璃反射" },
            new AcousticMaterial { Name = "厚窗帘", Alpha500 = 0.25, Alpha1000 = 0.35, Description = "褶皱厚布帘" },
            new AcousticMaterial { Name = "木地板", Alpha500 = 0.06, Alpha1000 = 0.07, Description = "普通木地板" },
            new AcousticMaterial { Name = "瓷砖/石材", Alpha500 = 0.01, Alpha1000 = 0.01, Description = "硬质地面" },
            new AcousticMaterial { Name = "厚地毯", Alpha500 = 0.30, Alpha1000 = 0.45, Description = "厚地毯+垫层" },
            new AcousticMaterial { Name = "石膏板吊顶", Alpha500 = 0.05, Alpha1000 = 0.06, Description = "普通吊顶" },
            new AcousticMaterial { Name = "吸音板", Alpha500 = 0.70, Alpha1000 = 0.80, Description = "50–100mm 多孔吸音" },
            new AcousticMaterial { Name = "扩散板", Alpha500 = 0.15, Alpha1000 = 0.18, Description = "二次余数扩散" },
            new AcousticMaterial { Name = "书架/唱片柜", Alpha500 = 0.18, Alpha1000 = 0.22, Description = "满架书籍/唱片" }
        };

        public static IEnumerable<string> Names
        {
            get { return Materials.Select(item => item.Name).Distinct().ToList(); }
        }

        public static AcousticMaterial Get(string name)
        {
            return Materials.FirstOrDefault(item => item.Name == name) ?? Materials[0];
        }

        public static double FurnitureAbsorption(string level)
        {
            if (level == "少") return 1.5;
            if (level == "多") return 7.0;
            return 4.0;
        }
    }
}
