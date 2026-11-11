using System;
using System.Windows;
using RoomTune.Core.Models;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class SyncWindow : Window
    {
        private readonly RoomProject _project;

        public SyncWindow(RoomProject project)
        {
            InitializeComponent();
            _project = project;
            if (project != null)
            {
                try
                {
                    CodeBox.Text = SyncCodec.Encode(project);
                    StatusText.Text = "当前项目已编码。";
                }
                catch (Exception ex)
                {
                    StatusText.Text = "编码失败：" + ex.Message;
                }
            }
            else
            {
                StatusText.Text = "粘贴手机端同步码后导入。";
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(CodeBox.Text ?? "");
                StatusText.Text = "已复制到剪贴板。";
            }
            catch (Exception ex)
            {
                StatusText.Text = "复制失败：" + ex.Message;
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CodeBox.Text = Clipboard.GetText();
                StatusText.Text = "已从剪贴板粘贴。";
            }
            catch (Exception ex)
            {
                StatusText.Text = "粘贴失败：" + ex.Message;
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = SyncCodec.Decode(CodeBox.Text);
                if (MessageBox.Show("导入“" + project.Name + "”为新项目？", "RoomTune",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                AppSession.ImportProject(project);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                StatusText.Text = "导入失败：" + ex.Message;
                MessageBox.Show("同步码无效或损坏：\r\n" + ex.Message, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
