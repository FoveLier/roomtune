using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Models;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class SettingsView : UserControl, IRefreshable
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            var config = AppSession.Config;
            BaseUrlBox.Text = config.AiBaseUrl;
            ModelBox.Text = config.AiModel;
            KeyBox.Password = config.AiApiKey;
            UseAiBox.IsChecked = config.UseAi;
            DataText.Text = "当前数据目录：" + ProjectRepository.DataRoot + "\r\n配置文件：" + ProjectRepository.ConfigPath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var config = new AppConfig
            {
                AiBaseUrl = BaseUrlBox.Text.Trim(),
                AiModel = ModelBox.Text.Trim(),
                AiApiKey = KeyBox.Password.Trim(),
                UseAi = UseAiBox.IsChecked == true,
                LastProjectId = AppSession.Current == null ? "" : AppSession.Current.Id,
                DataFolder = ProjectRepository.DataRoot
            };
            if (string.IsNullOrWhiteSpace(config.AiModel)) config.AiModel = "deepseek-chat";
            AppSession.SaveConfig(config);
            StatusText.Text = "设置已保存到 " + ProjectRepository.ConfigPath;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectRepository.EnsureFolders();
                Process.Start("explorer.exe", ProjectRepository.DataRoot);
            }
            catch (Exception ex)
            {
                StatusText.Text = "打开目录失败：" + ex.Message;
            }
        }
    }
}
