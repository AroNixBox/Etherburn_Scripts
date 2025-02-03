using UnityEngine;

namespace Game {
    public class GameOver : MonoBehaviour {
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _handler;
        private static bool _isQuitting;

        void OnEnable() {
            Application.quitting += OnApplicationQuit;
        }

        void OnDisable() {
            Application.quitting -= OnApplicationQuit;
        }

        void OnApplicationQuit() {
            // Safety since player unregisters on application quit aswell from the EntityManager,
            // so the event is called but we dont want to load the game over scene
            
            _isQuitting = true;
        }

        async void Start() {
            await EntityManager.Instance.WaitTillInitialized();
            _ = EntityManager.Instance
                .GetEntityOfType(EntityType.Player, out var targetEntityUnregisteredChannel).transform;

            _handler = LoadGameOverScene;
            targetEntityUnregisteredChannel.RegisterListener(_handler);
        }

        public void LoadGameOverScene() {
            if (_isQuitting) return;

            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }

            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.GameOver));
        }

        public void LoadMainMenuScene() {
            if (_isQuitting) return;

            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }

            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.Main));
        }
    }
}