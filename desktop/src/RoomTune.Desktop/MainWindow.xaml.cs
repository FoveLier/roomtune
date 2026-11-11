using System;
using System.Windows;
using System.Windows.Controls;
using RoomTune.Desktop.Services;
using RoomTune.Desktop.Views;

namespace RoomTune.Desktop
{
    public partial class MainWindow : Window
    {
        private ProjectsView _projectsView;
        private RoomView _roomView;
        private AnalysisView _analysisView;
        private PlanView _planView;
        private MeasurementView _measurementView;
        private ReportView _reportView;
        private SettingsView _settingsView;

        public MainWindow()
        {
            InitializeComponent();
            AppSession.Initialize();
            AppSession.ProjectChanged += RefreshAll;
            _projectsView = new ProjectsView();
            _roomView = new RoomView();
            _analysisView = new AnalysisView();
            _planView = new PlanView();
            _measurementView = new MeasurementView();
            _reportView = new ReportView();
            _settingsView = new SettingsView();
            _projectsView.OpenProjectRequested += project =>
            {
                AppSession.SelectProject(project.Id);
                NavList.SelectedIndex = 1;
                RefreshAll();
            };
            _projectsView.ImportSyncRequested += ImportSync;
            NavList.SelectedIndex = 0;
            RefreshAll();
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentHost == null) return;
            switch (NavList.SelectedIndex)
            {
                case 0: ContentHost.Content = _projectsView; break;
                case 1: ContentHost.Content = _roomView; break;
                case 2: ContentHost.Content = _analysisView; break;
                case 3: ContentHost.Content = _planView; break;
                case 4: ContentHost.Content = _measurementView; break;
                case 5: ContentHost.Content = _reportView; break;
                case 6: ContentHost.Content = _settingsView; break;
                default: ContentHost.Content = _projectsView; break;
            }
            var refreshable = ContentHost.Content as IRefreshable;
            if (refreshable != null) refreshable.RefreshView();
        }

        private void RefreshAll()
        {
            if (_projectsView == null) return;
            _projectsView.RefreshView();
            _roomView.RefreshView();
            _analysisView.RefreshView();
            _planView.RefreshView();
            _measurementView.RefreshView();
            _reportView.RefreshView();
            _settingsView.RefreshView();
            DataPathText.Text = "数据目录：" + RoomTune.Core.Services.ProjectRepository.DataRoot;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            ImportSync();
        }

        private void ImportSync()
        {
            var window = new SyncWindow(AppSession.Current) { Owner = this };
            if (window.ShowDialog() == true)
            {
                var refreshable = ContentHost.Content as IRefreshable;
                if (refreshable != null) refreshable.RefreshView();
                NavList.SelectedIndex = 1;
                RefreshAll();
            }
        }
    }
}
