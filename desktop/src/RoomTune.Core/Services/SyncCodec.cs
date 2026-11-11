using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using RoomTune.Core.Models;

namespace RoomTune.Core.Services
{
    public static class SyncCodec
    {
        public static string Encode(RoomProject project)
        {
            if (project == null) throw new ArgumentNullException("project");
            project.Normalize();
            var json = new JavaScriptSerializer().Serialize(project);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static RoomProject Decode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("同步码为空");
            var clean = code.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(clean));
            var map = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            if (map == null || !map.ContainsKey("Name") || !map.ContainsKey("Room") || !map.ContainsKey("System"))
                throw new InvalidOperationException("同步码缺少 Name/Room/System，无法导入");
            var project = new JavaScriptSerializer().Deserialize<RoomProject>(json);
            if (project == null || string.IsNullOrWhiteSpace(project.Name)) throw new InvalidOperationException("同步码内容无效");
            project.Normalize();
            return project;
        }
    }
}
