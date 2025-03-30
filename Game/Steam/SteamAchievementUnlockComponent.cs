using UnityEngine;

namespace Game.Steam {
    public class SteamAchievementUnlockComponent : MonoBehaviour {
        [SerializeField] SteamAchievements.SteamAchievement achievementKey;

        public void UnlockAchievement() {
            if (!Steamworks.SteamClient.IsValid) {
                Debug.LogError("Steam Client is not valid. Cannot unlock achievementKey.");
                return;
            }
            
            SteamAchievements.UnlockAchievement(achievementKey);
        }
    }
}
