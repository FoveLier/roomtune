using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RoomTune.Core.Models;
using RoomTune.Core.Services;

namespace RoomTune.Core.Tests
{
    internal static class Program
    {
        private static int _failed;
        private static int _passed;

        private static void Main()
        {
            try
            {
                TestModes();
                TestRt60AndSchroeder();
                TestSbirAndLayout();
                TestRecommendationsAndParameters();
                TestSyncCodec();
                TestRewCsv();
                TestReportExport();
                Console.WriteLine();
                Console.WriteLine("PASS " + _passed + " / FAIL " + _failed);
                Environment.ExitCode = _failed == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL " + ex);
                Environment.ExitCode = 1;
            }
        }

        private static void TestModes()
        {
            var modes = AcousticsEngine.CalculateModes(5.2, 3.8, 2.7, 300);
            Assert(modes.Count > 10, "mode count > 10");
            Assert(modes.Any(item => Math.Abs(item.FrequencyHz - 32.98) < 0.8), "5.2m axial mode ~32.98Hz");
            Assert(modes.All(item => item.FrequencyHz >= 16 && item.FrequencyHz <= 300), "mode range 16-300Hz");
            Assert(modes.Any(item => item.Type == "轴向"), "contains axial mode");
            Assert(modes.Any(item => item.Type == "切向" || item.Type == "斜向"), "contains non-axial mode");
            Assert(modes.Any(item => item.FrequencyHz > 280), "order limit covers near-300Hz mode");
        }

        private static void TestRt60AndSchroeder()
        {
            var project = RoomProject.CreateNew("测试客厅");
            var analysis = AcousticsEngine.Analyze(project);
            Assert(analysis.Rt60Seconds >= 0.15 && analysis.Rt60Seconds <= 1.8, "RT60 range");
            Assert(analysis.Rt60Low <= analysis.Rt60Seconds, "RT60 low bound");
            Assert(analysis.Rt60High >= analysis.Rt60Seconds, "RT60 high bound");
            Assert(analysis.SchroederHz > 20 && analysis.SchroederHz < 500, "Schroeder range");
            Assert(analysis.Modes.Count == analysis.ModeCount, "mode count copied");
            Assert(analysis.Risks.Count > 0, "risks generated");
        }

        private static void TestSbirAndLayout()
        {
            var project = RoomProject.CreateNew("测试摆位");
            project.System.Layout = "5.1.2";
            project.System.SubwooferCount = 2;
            project.System.SeedLayout(project.Room);
            var analysis = AcousticsEngine.Analyze(project);
            Assert(analysis.SbirIssues.Count > 0, "SBIR issues generated");
            Assert(analysis.Reflections.Count > 0, "reflection points generated");
            Assert(project.System.Speakers.Any(item => item.IsSubwoofer), "subwoofer seeded");
            Assert(project.System.Speakers.Count(item => item.Role == "左主箱") == 1, "left speaker exactly once");

            AcousticsEngine.ApplyBalancedLayout(project);
            var left = project.System.Speakers.First(item => item.Role == "左主箱");
            var right = project.System.Speakers.First(item => item.Role == "右主箱");
            Assert(Math.Abs(project.Room.ListenerX - project.Room.WidthM / 2) < 0.01, "listener centered");
            Assert(Math.Abs((project.Room.WidthM - left.X) - right.X) < 0.05, "symmetric speakers");

            var stereo = RoomProject.CreateNew("2.0测试");
            stereo.System.Layout = "2.0";
            stereo.System.SubwooferCount = 1;
            stereo.System.SeedLayout(stereo.Room);
            Assert(stereo.System.SubwooferCount == 0, "2.0 clears subwoofer count");
            Assert(stereo.System.Speakers.All(item => !item.IsSubwoofer), "2.0 has no subwoofer");

            var immersive = RoomProject.CreateNew("7.1.4测试");
            immersive.System.Layout = "7.1.4";
            immersive.System.SubwooferCount = 1;
            immersive.System.SeedLayout(immersive.Room);
            Assert(immersive.System.Speakers.Count(item => item.Role.Contains("天空")) == 4, "7.1.4 has four height speakers");

            var timestamp = project.UpdatedAt;
            project.Normalize();
            Assert(project.UpdatedAt == timestamp, "normalize does not mutate UpdatedAt");
        }

        private static void TestRecommendationsAndParameters()
        {
            var project = RoomProject.CreateNew("测试方案");
            var bundle = AcousticsEngine.BuildRecommendations(project);
            Assert(bundle.Schemes.Count == 3, "three placement schemes");
            Assert(bundle.Treatments.Count > 0, "treatments generated");
            Assert(bundle.Parameters.PeqSuggestions.Count > 0, "PEQ starts generated");
            Assert(!string.IsNullOrWhiteSpace(bundle.Parameters.Crossover), "crossover generated");
            Assert(!string.IsNullOrWhiteSpace(bundle.Report) && bundle.Report.Contains("RoomTune"), "report generated");
            Assert(bundle.Analysis.HeadroomDb > 0, "headroom computed");
            Assert(bundle.Analysis.RecommendedMaxBoostDb >= 0, "max boost valid");
        }

        private static void TestSyncCodec()
        {
            var project = RoomProject.CreateNew("同步测试");
            project.Room.LengthM = 6.1;
            project.System.SpeakerModel = "测试音箱";
            var code = SyncCodec.Encode(project);
            Assert(code.Length > 100, "sync code length");
            var decoded = SyncCodec.Decode(code);
            Assert(decoded.Name == "同步测试", "sync name");
            Assert(Math.Abs(decoded.Room.LengthM - 6.1) < 0.001, "sync room");
            Assert(decoded.System.SpeakerModel == "测试音箱", "sync speaker");
            try
            {
                SyncCodec.Decode(Convert.ToBase64String(Encoding.UTF8.GetBytes("{}")));
                Assert(false, "invalid sync rejected");
            }
            catch (InvalidOperationException)
            {
                Assert(true, "invalid sync rejected");
            }
        }

        private static void TestRewCsv()
        {
            var root = @"F:\RoomTuneData\tests";
            Directory.CreateDirectory(root);
            var path = Path.Combine(root, "sample-rew.csv");
            File.WriteAllText(path, "Frequency (Hz), SPL (dB)\r\n20,80.1\r\n30,78.4\r\n40,82.0\r\n50,79.9\r\n");
            var measurement = RewCsvParser.ParseFile(path);
            Assert(measurement.Points.Count == 4, "CSV points");
            Assert(Math.Abs(measurement.Points[0].FrequencyHz - 20) < 0.01, "CSV frequency");
            Assert(Math.Abs(measurement.Points[2].SplDb - 82.0) < 0.01, "CSV SPL");
            File.Delete(path);
        }

        private static void TestReportExport()
        {
            var project = RoomProject.CreateNew("导出测试");
            var bundle = AcousticsEngine.BuildRecommendations(project);
            var markdown = ReportExporter.SaveMarkdown(project, bundle.Report);
            var html = ReportExporter.SaveHtml(project, bundle.Report);
            Assert(File.Exists(markdown), "Markdown report written");
            Assert(File.Exists(html), "HTML report written");
            Assert(new FileInfo(markdown).FullName.StartsWith(@"F:\RoomTuneData\exports", StringComparison.OrdinalIgnoreCase), "report path on F:");
            File.Delete(markdown);
            File.Delete(html);
        }

        private static void Assert(bool condition, string name)
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine("  ok  " + name);
            }
            else
            {
                _failed++;
                Console.WriteLine("FAIL  " + name);
            }
        }
    }
}
