using System;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class ReportView : UserControl, IRefreshable
    {
        private string _aiText = "";
        private string _aiProjectId = "";

        public ReportView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            if (AppSession.Current == null) return;
            if (AppSession.Bundle == null) AppSession.RefreshBundle();
            if (_aiProjectId != AppSession.Current.Id)
            {
                _aiText = "";
                _aiProjectId = AppSession.Current.Id;
            }
            var report = AppSession.Bundle.Report;
            if (!string.IsNullOrWhiteSpace(_aiText))
            {
                report += "\r\n\r\n## AI 解释（可选）\r\n" + _aiText;
            }
            ReportBox.Text = report;
            StatusText.Text = "数据目录：" + ProjectRepository.DataRoot + "\\exports。AI 需要设置中的 API Key。";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _aiText = "";
            AppSession.RefreshBundle();
            RefreshView();
            StatusText.Text = "报告已按当前项目重新生成。";
        }

        private void ExportMarkdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ReportExporter.SaveMarkdown(AppSession.Current, ReportBox.Text);
                StatusText.Text = "已导出 Markdown：" + path;
            }
            catch (Exception ex)
            {
                StatusText.Text = "导出失败：" + ex.Message;
            }
        }

        private void ExportHtml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ReportExporter.SaveHtml(AppSession.Current, ReportBox.Text);
                StatusText.Text = "已导出 HTML：" + path;
            }
            catch (Exception ex)
            {
                StatusText.Text = "导出失败：" + ex.Message;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ReportBox.Text ?? "");
                StatusText.Text = "报告已复制到剪贴板。";
            }
            catch (Exception ex)
            {
                StatusText.Text = "复制失败：" + ex.Message;
            }
        }

        private async void Ai_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.Current == null) return;
            if (!AppSession.Config.UseAi)
            {
                MessageBox.Show("请先在“设置”中勾选“启用 AI 解释按钮”。未启用时本地报告完整可用。",
                    "RoomTune", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(AppSession.Config.AiApiKey))
            {
                MessageBox.Show("请先在“设置”中填写 OpenAI-compatible API Key；也可以继续使用本地报告。",
                    "RoomTune", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AiButton.IsEnabled = false;
            StatusText.Text = "正在请求 AI 解释…（不会上传原始测量 CSV，只发送房间摘要与报告）";
            var requestProjectId = AppSession.Current.Id;
            try
            {
                var aiText = await AiService.GenerateAsync(AppSession.Current, AppSession.Bundle.Analysis, AppSession.Bundle.Report, AppSession.Config);
                if (AppSession.Current == null || AppSession.Current.Id != requestProjectId)
                {
                    StatusText.Text = "项目已切换，丢弃上一次 AI 结果。";
                    return;
                }
                _aiText = aiText;
                _aiProjectId = requestProjectId;
                RefreshView();
                StatusText.Text = "AI 解释已追加到报告末尾；请把它当作解释，不要当作测量结果。";
            }
            catch (Exception ex)
            {
                StatusText.Text = "AI 失败：" + ex.Message;
                MessageBox.Show(StatusText.Text, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                AiButton.IsEnabled = true;
            }
        }
    }
}
