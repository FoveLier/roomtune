using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Models;
using RoomTune.Core.Services;
using RoomTune.Desktop.Controls;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class RoomView : UserControl, IRefreshable
    {
        public RoomView()
        {
            InitializeComponent();
            ShapeBox.ItemsSource = new[] { "矩形", "长条形", "近方形", "L形/开放式" };
            FurnitureBox.ItemsSource = new[] { "少", "中等", "多" };
            LayoutBox.ItemsSource = new[] { "2.0", "2.1", "5.1", "5.1.2", "7.1.4" };
            TargetUseBox.ItemsSource = new[] { "电影", "HiFi" };
            TargetCurveBox.ItemsSource = new[] { "平直", "Harman", "电影" };
            FloorBox.ItemsSource = MaterialLibrary.Names.ToList();
            WallBox.ItemsSource = MaterialLibrary.Names.ToList();
            CeilingBox.ItemsSource = MaterialLibrary.Names.ToList();
            PlanControl.ProjectChanged += OnPlanChanged;
        }

        public void RefreshView()
        {
            var project = AppSession.Current;
            if (project == null) return;
            project.Normalize();
            NameBox.Text = project.Name;
            LengthBox.Text = project.Room.LengthM.ToString("0.0");
            WidthBox.Text = project.Room.WidthM.ToString("0.0");
            HeightBox.Text = project.Room.HeightM.ToString("0.0");
            ShapeBox.SelectedItem = project.Room.Shape;
            FurnitureBox.SelectedItem = project.Room.FurnitureLevel;
            FloorBox.SelectedItem = project.Room.FloorMaterial;
            WallBox.SelectedItem = project.Room.WallMaterial;
            CeilingBox.SelectedItem = project.Room.CeilingMaterial;
            WindowBox.Text = project.Room.WindowRatioPercent.ToString("0");
            ListenerXBox.Text = project.Room.ListenerX.ToString("0.00");
            ListenerYBox.Text = project.Room.ListenerY.ToString("0.00");
            LayoutBox.SelectedItem = project.System.Layout;
            TargetUseBox.SelectedItem = project.System.TargetUse;
            ModelBox.Text = project.System.SpeakerModel;
            TargetCurveBox.SelectedItem = project.System.TargetCurve;
            SpeakerCountBox.Text = project.System.SpeakerCount.ToString();
            SubCountBox.Text = project.System.SubwooferCount.ToString();
            LowFreqBox.Text = project.System.LowFreqHz.ToString("0");
            HighFreqBox.Text = project.System.HighFreqHz.ToString("0");
            SensitivityBox.Text = project.System.SensitivityDb.ToString("0");
            MaxSplBox.Text = project.System.MaxSplDb.ToString("0");
            DynamicRangeBox.Text = project.System.DynamicRangeDb.ToString("0");
            ListeningLevelBox.Text = project.System.ListeningLevelDb.ToString("0");
            PlanControl.Project = project;
            StatusText.Text = "当前项目：" + project.Name + "。拖动平面图后请保存。";
        }

        private void OnPlanChanged()
        {
            if (AppSession.Current == null) return;
            ListenerXBox.Text = AppSession.Current.Room.ListenerX.ToString("0.00");
            ListenerYBox.Text = AppSession.Current.Room.ListenerY.ToString("0.00");
            StatusText.Text = "平面图已调整，点击“保存并更新分析”写入 F 盘。";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            try
            {
                ApplyForm(AppSession.Current);
                AppSession.Current.Normalize();
                AppSession.SaveCurrent();
                RefreshView();
                StatusText.Text = "已保存到 " + ProjectRepository.DataRoot + "，分析已刷新。";
            }
            catch (FormatException ex)
            {
                StatusText.Text = "数字格式有误：" + ex.Message;
                MessageBox.Show(StatusText.Text, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                StatusText.Text = "保存失败：" + ex.Message;
                MessageBox.Show(StatusText.Text, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyForm(RoomProject project)
        {
            var oldLayout = project.System.Layout;
            var oldSubwooferCount = project.System.SubwooferCount;
            project.Name = string.IsNullOrWhiteSpace(NameBox.Text) ? "未命名房间" : NameBox.Text.Trim();
            project.Room.LengthM = ReadDouble(LengthBox, project.Room.LengthM);
            project.Room.WidthM = ReadDouble(WidthBox, project.Room.WidthM);
            project.Room.HeightM = ReadDouble(HeightBox, project.Room.HeightM);
            project.Room.Shape = ShapeBox.SelectedItem == null ? project.Room.Shape : ShapeBox.SelectedItem.ToString();
            project.Room.FurnitureLevel = FurnitureBox.SelectedItem == null ? project.Room.FurnitureLevel : FurnitureBox.SelectedItem.ToString();
            project.Room.FloorMaterial = FloorBox.SelectedItem == null ? project.Room.FloorMaterial : FloorBox.SelectedItem.ToString();
            project.Room.WallMaterial = WallBox.SelectedItem == null ? project.Room.WallMaterial : WallBox.SelectedItem.ToString();
            project.Room.CeilingMaterial = CeilingBox.SelectedItem == null ? project.Room.CeilingMaterial : CeilingBox.SelectedItem.ToString();
            project.Room.WindowRatioPercent = ReadDouble(WindowBox, project.Room.WindowRatioPercent);
            project.Room.ListenerX = ReadDouble(ListenerXBox, project.Room.ListenerX);
            project.Room.ListenerY = ReadDouble(ListenerYBox, project.Room.ListenerY);
            project.System.Layout = LayoutBox.SelectedItem == null ? project.System.Layout : LayoutBox.SelectedItem.ToString();
            project.System.TargetUse = TargetUseBox.SelectedItem == null ? project.System.TargetUse : TargetUseBox.SelectedItem.ToString();
            project.System.SpeakerModel = string.IsNullOrWhiteSpace(ModelBox.Text) ? "通用音箱" : ModelBox.Text.Trim();
            project.System.TargetCurve = TargetCurveBox.SelectedItem == null ? project.System.TargetCurve : TargetCurveBox.SelectedItem.ToString();
            project.System.SpeakerCount = ReadInt(SpeakerCountBox, project.System.SpeakerCount);
            project.System.SubwooferCount = ReadInt(SubCountBox, project.System.SubwooferCount);
            project.System.LowFreqHz = ReadDouble(LowFreqBox, project.System.LowFreqHz);
            project.System.HighFreqHz = ReadDouble(HighFreqBox, project.System.HighFreqHz);
            project.System.SensitivityDb = ReadDouble(SensitivityBox, project.System.SensitivityDb);
            project.System.MaxSplDb = ReadDouble(MaxSplBox, project.System.MaxSplDb);
            project.System.DynamicRangeDb = ReadDouble(DynamicRangeBox, project.System.DynamicRangeDb);
            project.System.ListeningLevelDb = ReadDouble(ListeningLevelBox, project.System.ListeningLevelDb);
            if (project.System.Speakers == null || project.System.Speakers.Count == 0
                || oldLayout != project.System.Layout || oldSubwooferCount != project.System.SubwooferCount)
            {
                project.System.SeedLayout(project.Room);
            }
        }

        private static double ReadDouble(TextBox box, double fallback)
        {
            if (box == null) return fallback;
            double value;
            if (double.TryParse(box.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)) return value;
            if (double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value;
            throw new FormatException(box.Text + " 不是有效数字");
        }

        private static int ReadInt(TextBox box, int fallback)
        {
            int value;
            if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)) return value;
            if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return value;
            throw new FormatException(box.Text + " 不是有效整数");
        }

        private void ResetLayout_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            AppSession.Current.System.SeedLayout(AppSession.Current.Room);
            AppSession.SaveCurrent();
            RefreshView();
            StatusText.Text = "已按当前布局重置音箱位置。";
        }

        private void Balanced_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            AcousticsEngine.ApplyBalancedLayout(AppSession.Current);
            AppSession.SaveCurrent();
            RefreshView();
            StatusText.Text = "已应用对称等边摆位；可用测量页导入 REW 复核。";
        }
    }
}
