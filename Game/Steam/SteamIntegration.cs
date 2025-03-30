using System;
using Extensions;
using UnityEngine;

namespace Game.Steam {
    public class SteamIntegration : Singleton<SteamIntegration> {
        const int SteamAppId = 3357430;
        void Start() {
            try {
                Steamworks.SteamClient.Init(SteamAppId);
            }
            catch (Exception e) {
                Debug.LogError("Steam Client Initialization failed: " + e.Message);
            }
        }

        void Update() {
            try {
                Steamworks.SteamClient.RunCallbacks();
            }
            catch (Exception e) {
                Debug.LogError("Steam Client RunCallbacks failed: " + e.Message);
            }
        }

        void OnApplicationQuit() {
            try {
                Steamworks.SteamClient.Shutdown();
            }
            catch (Exception e) {
                Debug.LogError("Steam Client Shutdown failed: " + e.Message);
            }
        }
    }
}
