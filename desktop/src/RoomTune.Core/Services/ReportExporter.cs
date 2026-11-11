using System;
using System.IO;
using System.Text;
using RoomTune.Core.Models;

namespace RoomTune.Core.Services
{
    public static class ReportExporter
    {
        public static string SaveMarkdown(RoomProject project, string markdown)
        {
            ProjectRepository.EnsureFolders();
            var safeName = MakeSafeFileName(project == null ? "roomtune" : project.Name);
            var path = Path.Combine(ProjectRepository.ExportsDirectory,
                safeName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md");
            File.WriteAllText(path, markdown ?? "", new UTF8Encoding(false));
            return path;
        }

        public static string SaveHtml(RoomProject project, string markdown)
        {
            ProjectRepository.EnsureFolders();
            var safeName = MakeSafeFileName(project == null ? "roomtune" : project.Name);
            var path = Path.Combine(ProjectRepository.ExportsDirectory,
                safeName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".html");
            var html = "<!doctype html><html lang=\"zh-CN\"><head><meta charset=\"utf-8\"><title>" +
                       Escape(project == null ? "RoomTune" : project.Name) +
                       "</title><style>body{font-family:Segoe UI,Microsoft YaHei,sans-serif;max-width:900px;margin:40px auto;line-height:1.7;color:#20242c;background:#f6f7f9;padding:24px}pre{white-space:pre-wrap;background:#fff;padding:20px;border:1px solid #dde2ea;border-radius:8px}</style></head><body><pre>" +
                       Escape(markdown) + "</pre></body></html>";
            File.WriteAllText(path, html, new UTF8Encoding(false));
            return path;
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return string.IsNullOrWhiteSpace(name) ? "roomtune" : name.Trim();
        }

        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
