using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class AnalysisView : UserControl, IRefreshable
    {
        public AnalysisView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            if (AppSession.Current == null) return;
            if (AppSession.Bundle == null) AppSession.RefreshBundle();
            var analysis = AppSession.Bundle.Analysis;
            VolumeText.Text = analysis.VolumeM3.ToString("0.0") + " m³";
            Rt60Text.Text = analysis.Rt60Seconds.ToString("0.00") + " s\r\n" +
                            analysis.Rt60Low.ToString("0.00") + "–" + analysis.Rt60High.ToString("0.00") + " s";
            Rt60Text.FontSize = 18;
            SchroederText.Text = analysis.SchroederHz.ToString("0") + " Hz";
            HeadroomText.Text = analysis.HeadroomDb.ToString("0.0") + " dB";
            ModeCountText.Text = analysis.ModeCount.ToString();
            ConfidenceText.Text = analysis.Confidence;
            ModeChart.Modes = analysis.Modes;
            ModeList.ItemsSource = analysis.Modes.Select(item => item.ToString()).ToList();
            RiskList.ItemsSource = analysis.Risks.Count == 0
                ? new[] { "未发现明显高风险项。" }
                : analysis.Risks.Select(item => item.ToString()).ToList();
            SbirList.ItemsSource = analysis.SbirIssues
                .Select(item => string.Format("{0} {1}：{2:0.00}m → {3:0} Hz（{4}）",
                    item.SpeakerRole, item.Surface, item.DistanceM, item.CancellationHz, item.Severity))
                .ToList();
            ReflectionList.ItemsSource = analysis.Reflections
                .Select(item => string.Format("{0} {1}：({2:0.0}, {3:0.0})，路径 {4:0.0}m",
                    item.SpeakerRole, item.Surface, item.X, item.Y, item.DistanceM))
                .ToList();
            StatusText.Text = "按当前房间档案计算；未导入 REW 时为估算。";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            AppSession.RefreshBundle();
            RefreshView();
            StatusText.Text = "已重新计算。";
        }
    }
}
