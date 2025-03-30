using System.Collections.Generic;
using UnityEngine;

namespace Game.Steam {
    public class SteamAchievements : MonoBehaviour {
        public enum SteamAchievement {
            BBQed,
            HowDidIGetHere,
            BiggerSword
        }
        static readonly Dictionary<SteamAchievement, string> AchievementData = new() {
            { SteamAchievement.BBQed, "ACH_BBQED" },
            { SteamAchievement.HowDidIGetHere, "ACH_HOW_DID_I_GET_HERE" },
            { SteamAchievement.BiggerSword, "ACH_BIGGER_SWORD" }
        };
        
        public static void UnlockAchievement(SteamAchievement achievementKey) {
            if (!Steamworks.SteamClient.IsValid) {
                Debug.LogError("Steam Client is not valid. Cannot unlock achievementKey.");
                return;
            }
            
            var achievementId = AchievementData[achievementKey];
            
            if(IsAchievementAlreadyUnlocked(achievementId)) { return; }
            
            var achievement = new Steamworks.Data.Achievement(achievementId);
            achievement.Trigger();
        }
        static bool IsAchievementAlreadyUnlocked(string achievementId) {
            var achievement = new Steamworks.Data.Achievement(achievementId);
            return achievement.State;
        }

        public static void ClearAchievements() {
            // TODO: Handle only clearing specific achievements ...
        }
    }
}
