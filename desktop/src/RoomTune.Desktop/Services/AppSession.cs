using System;
using System.Collections.Generic;
using System.Linq;
using RoomTune.Core.Models;
using RoomTune.Core.Services;

namespace RoomTune.Desktop.Services
{
    public static class AppSession
    {
        public static AppConfig Config { get; private set; }
        public static RoomProject Current { get; private set; }
        public static List<RoomProject> Projects { get; private set; }
        public static RecommendationBundle Bundle { get; private set; }

        public static event Action ProjectChanged;

        public static void Initialize()
        {
            ProjectRepository.EnsureFolders();
            Config = ProjectRepository.LoadConfig();
            Projects = ProjectRepository.LoadAll();
            if (Projects.Count == 0)
            {
                var sample = RoomProject.CreateNew("我的客厅影院");
                sample.Notes = "示例房间档案，可直接修改尺寸、材质和音箱参数。";
                ProjectRepository.Save(sample);
                Projects = ProjectRepository.LoadAll();
            }
            Current = Projects.FirstOrDefault(item => item.Id == Config.LastProjectId) ?? Projects.First();
            RefreshBundle();
        }

        public static void RefreshBundle()
        {
            if (Current == null) return;
            Bundle = AcousticsEngine.BuildRecommendations(Current);
        }

        public static void ReloadProjects()
        {
            Projects = ProjectRepository.LoadAll();
            if (Current == null || Projects.All(item => item.Id != Current.Id))
            {
                Current = Projects.FirstOrDefault();
            }
        }

        public static void SelectProject(string id)
        {
            var project = ProjectRepository.Load(id);
            if (project == null) return;
            Current = project;
            Config.LastProjectId = project.Id;
            ProjectRepository.SaveConfig(Config);
            RefreshBundle();
            OnProjectChanged();
        }

        public static RoomProject CreateProject(string name)
        {
            var project = RoomProject.CreateNew(string.IsNullOrWhiteSpace(name) ? "新房间" : name.Trim());
            ProjectRepository.Save(project);
            Current = project;
            Config.LastProjectId = project.Id;
            ProjectRepository.SaveConfig(Config);
            ReloadProjects();
            RefreshBundle();
            OnProjectChanged();
            return project;
        }

        public static void SaveCurrent()
        {
            if (Current == null) return;
            ProjectRepository.Save(Current);
            Config.LastProjectId = Current.Id;
            ProjectRepository.SaveConfig(Config);
            ReloadProjects();
            RefreshBundle();
            OnProjectChanged();
        }

        public static void DeleteProject(string id)
        {
            ProjectRepository.Delete(id);
            ReloadProjects();
            if (Projects.Count == 0)
            {
                CreateProject("新房间");
            }
            else if (Current == null)
            {
                Current = Projects.First();
            }
            Config.LastProjectId = Current == null ? "" : Current.Id;
            ProjectRepository.SaveConfig(Config);
            RefreshBundle();
            OnProjectChanged();
        }

        public static void ImportProject(RoomProject project)
        {
            if (project == null) return;
            project.Id = Guid.NewGuid().ToString("N");
            project.Name = project.Name + "（同步）";
            ProjectRepository.Save(project);
            Current = project;
            Config.LastProjectId = project.Id;
            ProjectRepository.SaveConfig(Config);
            ReloadProjects();
            RefreshBundle();
            OnProjectChanged();
        }

        public static void SaveConfig(AppConfig config)
        {
            Config = config;
            ProjectRepository.SaveConfig(Config);
        }

        private static void OnProjectChanged()
        {
            var handler = ProjectChanged;
            if (handler != null) handler();
        }
    }
}
