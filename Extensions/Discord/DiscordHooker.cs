using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Discord {
    public static class DiscordHooker {
        public static async Task SendToDiscord(WebhookSO webhookSo, string message) {
            try {
                using var client = new HttpClient();
                
                var mentions = string.Join(" ", webhookSo.Members.Select(member => $"<@{member.id}>"));
        
                var payload = new {
                    content = $"{mentions} \n 🚨 **{Application.productName}** Build Error:\n\n{message}"
                };
        
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);            
                var content = new StringContent(json, Encoding.UTF8, "application/json");
        
                _ = await client.PostAsync(webhookSo.WebHookUrl, content);
            }
            catch {
                Debug.LogWarning("Discord-Message couldn't be sent");
            }
        }
    }
}
