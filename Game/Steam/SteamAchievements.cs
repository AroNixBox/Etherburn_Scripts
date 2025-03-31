using System.Collections.Generic;
using UnityEngine;

namespace Game.Steam {
    public class SteamAchievements : MonoBehaviour {
        public enum SteamAchievement {
            BBQed,
            HowDidIGetHere,
            BiggerSword,
            LockedAndLoaded,
            DragonSlayer,
            StillCountsAsOne,
            DeathIsOnlyTheBeginning,
        }
        static readonly Dictionary<SteamAchievement, string> AchievementData = new() {
            { SteamAchievement.BBQed, "ACH_BBQED" },
            { SteamAchievement.HowDidIGetHere, "ACH_HOW_DID_I_GET_HERE" },
            { SteamAchievement.BiggerSword, "ACH_BIGGER_SWORD" },
            { SteamAchievement.LockedAndLoaded, "ACH_LOCKED_AND_LOADED" },
            { SteamAchievement.DragonSlayer, "ACH_DRAGON_SLAYER" },
            { SteamAchievement.StillCountsAsOne, "ACH_STILL_COUNTS_AS_ONE" },
            { SteamAchievement.DeathIsOnlyTheBeginning, "ACH_DEATH_IS_ONLY_THE_BEGINNING" },
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
