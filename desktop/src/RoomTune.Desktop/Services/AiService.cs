using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RoomTune.Core.Models;
using RoomTune.Core.Services;

namespace RoomTune.Desktop.Services
{
    public static class AiService
    {
        public static async Task<string> GenerateAsync(RoomProject project, AnalysisResult analysis, string localReport, AppConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.AiApiKey))
                throw new InvalidOperationException("请先在设置中填写 AI API Key。");

            var baseUrl = (config.AiBaseUrl ?? "").Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("AI Base URL 为空。");
            var endpoint = baseUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                ? baseUrl
                : baseUrl + "/chat/completions";

            var context = string.Format(
                "房间：{0:0.0}×{1:0.0}×{2:0.0}m，体积 {3:0.0}m³；RT60 {4:0.00}s；Schroeder {5:0}Hz；系统 {6}，主箱 {7}，低音炮 {8} 只；动态余量 {9:0.0}dB。风险：{10}",
                project.Room.LengthM, project.Room.WidthM, project.Room.HeightM,
                analysis.VolumeM3, analysis.Rt60Seconds, analysis.SchroederHz,
                project.System.Layout, project.System.SpeakerModel, project.System.SubwooferCount,
                analysis.HeadroomDb,
                string.Join("；", analysis.Risks.Select(item => item.Severity + "·" + item.Title)));

            var userPrompt = "请基于下面的确定性声学结果，用中文写一份 300–500 字的家庭影院/HiFi 声学解释。\n" +
                             "必须：先给 3 条按优先级排序的行动；说明为什么；指出哪些结论是估算、下一步要实测什么；不要编造新的频率/参数；不要建议购买具体品牌。\n\n" +
                             "项目：" + project.Name + "\n" + context + "\n\n本地报告：\n" + localReport;

            var payload = new Dictionary<string, object>
            {
                { "model", config.AiModel },
                { "temperature", 0.3 },
                { "messages", new object[]
                    {
                        new Dictionary<string, object> { { "role", "system" }, { "content", "你是谨慎、可解释的家庭声学顾问。只解释已给出的数据，不虚构测量。" } },
                        new Dictionary<string, object> { { "role", "user" }, { "content", userPrompt } }
                    }
                }
            };
            var json = new JavaScriptSerializer().Serialize(payload);
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(45);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + config.AiApiKey);
                using (var response = await client.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json")))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException("AI 服务返回 " + (int)response.StatusCode + "：" + Shorten(body, 240));
                    }
                    var parsed = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(body);
                    object choicesObject;
                    if (!parsed.TryGetValue("choices", out choicesObject) || !(choicesObject is object[]))
                        throw new InvalidOperationException("AI 返回格式中没有 choices。");
                    var choices = (object[])choicesObject;
                    if (choices.Length == 0) throw new InvalidOperationException("AI 返回 choices 为空。");
                    var first = choices[0] as Dictionary<string, object>;
                    if (first == null) throw new InvalidOperationException("AI 返回 choices 格式异常。");
                    object messageObject;
                    if (!first.TryGetValue("message", out messageObject) || !(messageObject is Dictionary<string, object>))
                        throw new InvalidOperationException("AI 返回缺少 message。");
                    var message = (Dictionary<string, object>)messageObject;
                    object content;
                    if (!message.TryGetValue("content", out content) || content == null)
                        throw new InvalidOperationException("AI 返回缺少 content。");
                    return content.ToString();
                }
            }
        }

        private static string Shorten(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= max ? text : text.Substring(0, max) + "…";
        }
    }
}
