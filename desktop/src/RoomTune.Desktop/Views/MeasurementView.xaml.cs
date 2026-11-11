using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class MeasurementView : UserControl, IRefreshable
    {
        public MeasurementView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            if (AppSession.Current == null) return;
            var measurement = AppSession.Current.Measurement;
            ResponseChart.Measurement = measurement;
            if (measurement != null && measurement.HasData)
            {
                MetaText.Text = string.Format("来源：{0} · 导入：{1} · 点数：{2} · 频率范围：{3:0}–{4:0} Hz",
                    measurement.Source, measurement.ImportedAt, measurement.Points.Count,
                    measurement.Points[0].FrequencyHz, measurement.Points[measurement.Points.Count - 1].FrequencyHz);
                StatusText.Text = "已导入实测参考；分析页的置信度会更新。";
            }
            else
            {
                MetaText.Text = "尚未导入测量。当前所有分析为估算，请以房间尺寸/材质和摆位建议为起点。";
                StatusText.Text = "导入 REW CSV 后可保存到当前项目。";
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV / 文本测量文件 (*.csv;*.txt)|*.csv;*.txt|所有文件 (*.*)|*.*",
                Title = "选择 REW 导出的频率响应 CSV"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                if (AppSession.Current == null) return;
                AppSession.Current.Measurement = RewCsvParser.ParseFile(dialog.FileName);
                AppSession.SaveCurrent();
                RefreshView();
                StatusText.Text = "已导入：" + dialog.FileName;
            }
            catch (Exception ex)
            {
                StatusText.Text = "导入失败：" + ex.Message;
                MessageBox.Show(StatusText.Text, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            if (MessageBox.Show("清除当前项目的测量数据？", "RoomTune", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            AppSession.Current.Measurement = new RoomTune.Core.Models.MeasurementData();
            AppSession.SaveCurrent();
            RefreshView();
            StatusText.Text = "测量已清除；分析回到估算模式。";
        }
    }
}
