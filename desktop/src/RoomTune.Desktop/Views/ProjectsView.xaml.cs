using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Core.Models;
using RoomTune.Core.Services;
using RoomTune.Desktop.Services;

namespace RoomTune.Desktop.Views
{
    public partial class ProjectsView : UserControl, IRefreshable
    {
        public event Action<RoomProject> OpenProjectRequested;
        public event Action ImportSyncRequested;

        public ProjectsView()
        {
            InitializeComponent();
        }

        public void RefreshView()
        {
            var selectedId = ProjectList.SelectedItem is RoomProject ? ((RoomProject)ProjectList.SelectedItem).Id : null;
            ProjectList.ItemsSource = null;
            ProjectList.ItemsSource = AppSession.Projects;
            if (selectedId != null)
            {
                foreach (var item in AppSession.Projects)
                {
                    if (item.Id == selectedId)
                    {
                        ProjectList.SelectedItem = item;
                        break;
                    }
                }
            }
            else if (AppSession.Current != null)
            {
                foreach (var item in AppSession.Projects)
                {
                    if (item.Id == AppSession.Current.Id)
                    {
                        ProjectList.SelectedItem = item;
                        break;
                    }
                }
            }
            DataFolderText.Text = ProjectRepository.DataRoot + "\r\nprojects\\*.json\r\nexports\\*.md / *.html";
            UpdateDetails(ProjectList.SelectedItem as RoomProject ?? AppSession.Current);
        }

        private void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDetails(ProjectList.SelectedItem as RoomProject);
        }

        private void UpdateDetails(RoomProject project)
        {
            if (project == null)
            {
                DetailsText.Text = "请选择一个项目。";
                return;
            }
            DetailsText.Text = string.Format(
                "{0}\r\n\r\n房间：{1:0.0} × {2:0.0} × {3:0.0} m · {4}\r\n系统：{5} · {6} · 低音炮 {7} 只\r\nRT60：{8:0.00} s（估算）\r\n更新：{9}",
                project.Name, project.Room.LengthM, project.Room.WidthM, project.Room.HeightM, project.Room.Shape,
                project.System.Layout, project.System.SpeakerModel, project.System.SubwooferCount,
                AcousticsEngine.EstimateRt60(project.Room), project.UpdatedAt);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = AppSession.CreateProject(NewName.Text);
                NewName.Text = "新房间";
                RefreshView();
                ProjectList.SelectedItem = project;
            }
            catch (Exception ex)
            {
                MessageBox.Show("新建项目失败：" + ex.Message, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var project = ProjectList.SelectedItem as RoomProject;
            if (project == null)
            {
                MessageBox.Show("请先选择一个项目。", "RoomTune", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var handler = OpenProjectRequested;
            if (handler != null) handler(project);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var project = ProjectList.SelectedItem as RoomProject;
            if (project == null) return;
            if (MessageBox.Show("删除项目“" + project.Name + "”？此操作不可撤销。", "RoomTune",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                AppSession.DeleteProject(project.Id);
                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败：" + ex.Message, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ImportSync_Click(object sender, RoutedEventArgs e)
        {
            var handler = ImportSyncRequested;
            if (handler != null) handler();
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
                MessageBox.Show("无法打开数据目录：" + ex.Message, "RoomTune", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
