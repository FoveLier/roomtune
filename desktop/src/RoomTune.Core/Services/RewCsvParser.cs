using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RoomTune.Core.Models;

namespace RoomTune.Core.Services
{
    public static class RewCsvParser
    {
        public static MeasurementData ParseFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("找不到 CSV 文件", path);
            var lines = File.ReadAllLines(path);
            var points = new List<FrequencyPoint>();
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#") || line.StartsWith("*") || line.StartsWith("//")) continue;
                var parts = line.Split(new[] { ',', ';', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                double frequency;
                double spl;
                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out frequency)) continue;
                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out spl)) continue;
                if (frequency <= 0 || frequency > 40000) continue;
                if (double.IsNaN(spl) || double.IsInfinity(spl)) continue;
                points.Add(new FrequencyPoint { FrequencyHz = Math.Round(frequency, 2), SplDb = Math.Round(spl, 2) });
            }
            if (points.Count == 0) throw new InvalidDataException("CSV 中没有可用的“频率, dB”数据行");
            points = points.OrderBy(item => item.FrequencyHz).ToList();
            return new MeasurementData
            {
                Source = Path.GetFileName(path),
                ImportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Points = points
            };
        }
    }
}
