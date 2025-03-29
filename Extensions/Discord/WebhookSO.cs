using UnityEngine;

namespace Extensions.Discord {
    [PreferBinarySerialization]
    [CreateAssetMenu(fileName = "Webhook", menuName = "Webhook", order = 0)]
    public class WebhookSO : ScriptableObject {
        [SerializeField] string webHookUrl;
        [SerializeField] DiscordMember[] members;
        public string WebHookUrl {
            get {
                if (string.IsNullOrEmpty(webHookUrl)) {
                    Debug.LogError("Webhook URL is not set. Please set it in the inspector.");
                }
                return webHookUrl;
            }
        }
        
        public DiscordMember[] Members {
            get {
                if (members == null || members.Length == 0) {
                    Debug.LogError("No members found. Please add members in the inspector.");
                }
                return members;
            }
        }
        
        [System.Serializable]
        public class DiscordMember {
            public string name;
            public long id;
        }
    }
}