using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using RoomTune.Core.Models;

namespace RoomTune.Core.Services
{
    public static class ProjectRepository
    {
        public static string DataRoot = @"F:\RoomTuneData";
        public static string ProjectsDirectory { get { return Path.Combine(DataRoot, "projects"); } }
        public static string ExportsDirectory { get { return Path.Combine(DataRoot, "exports"); } }
        public static string LogsDirectory { get { return Path.Combine(DataRoot, "logs"); } }
        public static string ConfigPath { get { return Path.Combine(DataRoot, "config.json"); } }

        public static void EnsureFolders()
        {
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory(ProjectsDirectory);
            Directory.CreateDirectory(ExportsDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }

        public static List<RoomProject> LoadAll()
        {
            EnsureFolders();
            var list = new List<RoomProject>();
            foreach (var file in Directory.GetFiles(ProjectsDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file, Encoding.UTF8);
                    var project = new JavaScriptSerializer().Deserialize<RoomProject>(json);
                    if (project != null)
                    {
                        project.Normalize();
                        list.Add(project);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("加载项目失败：" + file + " · " + ex.Message);
                }
            }
            return list.OrderByDescending(item => item.UpdatedAt).ToList();
        }

        public static RoomProject Load(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var path = Path.Combine(ProjectsDirectory, id + ".json");
            if (!File.Exists(path)) return null;
            try
            {
                var project = new JavaScriptSerializer().Deserialize<RoomProject>(File.ReadAllText(path, Encoding.UTF8));
                if (project != null) project.Normalize();
                return project;
            }
            catch (Exception ex)
            {
                WriteLog("加载项目失败：" + ex.Message);
                return null;
            }
        }

        public static void Save(RoomProject project)
        {
            if (project == null) return;
            EnsureFolders();
            project.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            project.Normalize();
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(project);
            var path = Path.Combine(ProjectsDirectory, project.Id + ".json");
            File.WriteAllText(path, PrettyJson(json), new UTF8Encoding(false));
        }

        public static void Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            var path = Path.Combine(ProjectsDirectory, id + ".json");
            if (File.Exists(path)) File.Delete(path);
        }

        public static AppConfig LoadConfig()
        {
            EnsureFolders();
            if (!File.Exists(ConfigPath)) return new AppConfig { DataFolder = DataRoot };
            try
            {
                var config = new JavaScriptSerializer().Deserialize<AppConfig>(File.ReadAllText(ConfigPath, Encoding.UTF8));
                return config ?? new AppConfig { DataFolder = DataRoot };
            }
            catch
            {
                return new AppConfig { DataFolder = DataRoot };
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            EnsureFolders();
            config.DataFolder = DataRoot;
            var json = new JavaScriptSerializer().Serialize(config);
            File.WriteAllText(ConfigPath, PrettyJson(json), new UTF8Encoding(false));
        }

        public static void WriteLog(string message)
        {
            try
            {
                EnsureFolders();
                File.AppendAllText(Path.Combine(LogsDirectory, "roomtune.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine,
                    new UTF8Encoding(false));
            }
            catch
            {
                // Logging must never crash the UI.
            }
        }

        public static string PrettyJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            var sb = new StringBuilder();
            int indent = 0;
            bool inString = false;
            bool escaped = false;
            foreach (char c in json)
            {
                if (inString)
                {
                    sb.Append(c);
                    if (escaped) escaped = false;
                    else if (c == '\\') escaped = true;
                    else if (c == '"') inString = false;
                    continue;
                }
                if (c == '"')
                {
                    inString = true;
                    sb.Append(c);
                }
                else if (c == '{' || c == '[')
                {
                    sb.Append(c);
                    sb.AppendLine();
                    indent++;
                    sb.Append(new string(' ', indent * 2));
                }
                else if (c == '}' || c == ']')
                {
                    sb.AppendLine();
                    indent = Math.Max(0, indent - 1);
                    sb.Append(new string(' ', indent * 2));
                    sb.Append(c);
                }
                else if (c == ',')
                {
                    sb.Append(c);
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 2));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
