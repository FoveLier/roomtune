using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Models;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class PlanView : UserControl, IRefreshable
    {
        public PlanView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            if (AppSession.Current == null) return;
            if (AppSession.Bundle == null) AppSession.RefreshBundle();
            SchemeList.ItemsSource = AppSession.Bundle.Schemes;
            TreatmentList.ItemsSource = AppSession.Bundle.Treatments;
            ParameterBox.Text = AppSession.Bundle.Parameters.ToText();
            StatusText.Text = "已保存方案：" + AppSession.Current.SavedSchemes.Count + " 个。参数卡可复制到 AVR/Dirac/Audyssey/REW 作为起点。";
        }

        private void ApplyBalanced_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            AcousticsEngine.ApplyBalancedLayout(AppSession.Current);
            AppSession.SaveCurrent();
            RefreshView();
            StatusText.Text = "已应用对称等边摆位；请用测量页导入 REW 后再微调。";
        }

        private void SaveScheme_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            try
            {
                var scheme = AppSession.Bundle.Schemes.FirstOrDefault(item => item.Kind == "free") ?? AppSession.Bundle.Schemes.FirstOrDefault();
                if (scheme == null) return;
                var saved = new SavedScheme
                {
                    Name = "方案 " + (AppSession.Current.SavedSchemes.Count + 1) + " · " + scheme.Name,
                    Kind = scheme.Kind,
                    Summary = scheme.Summary + " " + scheme.StepsText,
                    ParameterCard = AppSession.Bundle.Parameters.ToText()
                };
                AppSession.Current.SavedSchemes.Add(saved);
                AppSession.SaveCurrent();
                RefreshView();
                StatusText.Text = "已保存方案快照：" + saved.Name;
            }
            catch (Exception ex)
            {
                StatusText.Text = "保存方案失败：" + ex.Message;
                MessageBox.Show(StatusText.Text, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SyncExport_Click(object sender, RoutedEventArgs e)
        {
            var window = new SyncWindow(AppSession.Current) { Owner = Window.GetWindow(this) };
            window.ShowDialog();
        }

        private void CopyCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ParameterBox.Text ?? "");
                StatusText.Text = "参数卡已复制到剪贴板。";
            }
            catch (Exception ex)
            {
                StatusText.Text = "复制失败：" + ex.Message;
            }
        }
    }
}
