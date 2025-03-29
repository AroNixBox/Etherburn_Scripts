using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions.Discord {
    /// <summary>
    /// Sits on a GameObject in the Bootstrapper Scene.
    /// </summary>
    public class ErrorHandler : Singleton<ErrorHandler> {
        [Tooltip("Enable automatic error reporting to Discord")]
        [SerializeField] WebhookSO[] discordWebhooks;
        [SerializeField] bool autoReportErrors = true;
        [SerializeField] bool onlyReportInBuild = true; 
        protected override bool ShouldPersist => true;
        
        void OnEnable() {
            if(onlyReportInBuild && Application.isEditor) { return; }
            
            if (autoReportErrors) {
                Application.logMessageReceived += HandleLogMessage;
            }
        }
        
        void OnDisable() {
            if(onlyReportInBuild && Application.isEditor) { return; }

            if (autoReportErrors) {
                Application.logMessageReceived -= HandleLogMessage;
            }
        }
        
        void HandleLogMessage(string logString, string stackTrace, LogType type) {
            // Only send errors and exceptions to Discord
            if (type != LogType.Error && type != LogType.Exception) { return; }
            // Current timestamp
            var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
            // Build detailed error message with formatting
            var errorMessage = $"**Timestamp:** ```csharp\n{timestamp}```\n" +
                               $"**Error Type:** ```csharp\n{type}```\n" +
                               $"**Message:** ```csharp\n{logString}```\n" +
                               $"**Stack Trace:** ```csharp\n{stackTrace}```";
        
            // Try to extract GameObject info from error context if available
            var activeObjects = FindActiveContextObjects(logString);
            if (!string.IsNullOrEmpty(activeObjects)) {
                errorMessage = $"**Relevant GameObjects:** {activeObjects}\n" + errorMessage;
            }

            foreach (var discordWebhook in discordWebhooks) {
                _ = DiscordHooker.SendToDiscord(discordWebhook, errorMessage);
            }
        }

        string FindActiveContextObjects(string errorMessage) {
            try {
                // Look for GameObject names mentioned in the error
                // This is a basic implementation that could be enhanced
                var activeObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(go => go.activeInHierarchy && errorMessage.Contains(go.name))
                    .Select(go => go.name)
                    .Distinct();
            
                return string.Join(", ", activeObjects);
            }
            catch {
                return string.Empty;
            }
        }
        
        // Add this to your ErrorHandler.cs class
        void OnApplicationQuit() {
            
        }
        
        [Button]
        void SendErrorToDiscord() {
            string errorMessage = "This is a test error message.";
            foreach (var discordWebhook in discordWebhooks) {
                _ = DiscordHooker.SendToDiscord(discordWebhook, errorMessage);
            }
        }
    }
}