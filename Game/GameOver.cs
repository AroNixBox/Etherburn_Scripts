using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class GameOver : MonoBehaviour {
        [SerializeField] Image fadeOutImage;
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _handler;
        static bool isQuitting;

        void OnEnable() {
            Application.quitting += OnApplicationQuit;
        }

        void OnDisable() {
            Application.quitting -= OnApplicationQuit;
        }

        void OnApplicationQuit() {
            // Safety since player unregisters on application quit aswell from the EntityManager,
            // so the event is called but we dont want to load the game over scene
            
            isQuitting = true;
        }

        async void Start() {
            await EntityManager.Instance.WaitTillInitialized();
            _ = EntityManager.Instance
                .GetEntityOfType(EntityType.Player, out var targetEntityUnregisteredChannel).transform;

            _handler = LoadGameOverScene;
            targetEntityUnregisteredChannel.RegisterListener(_handler);
        }

        public async void LoadGameOverScene() {
            if (isQuitting) return;

            await FadeInAsync();
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }

            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.GameOver));
        }

        async Task FadeInAsync() {
            var duration = 1f;
            var elapsedTime = 0f;
            var color = fadeOutImage.color;
            color.a = 0;
            fadeOutImage.color = color;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                fadeOutImage.color = color;
                await Task.Yield();
            }
        }
        public void LoadMainMenuScene() {
            if (isQuitting) return;

            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }

            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.Main));
        }
    }
}