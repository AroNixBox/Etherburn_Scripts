using Enemy.Positioning;
using UnityEngine;

namespace Game {
    public class GameOver : MonoBehaviour {
        TargetEntitiesUnregisteredChannel.TargetEntitiesUnregisteredChannelEventHandler _handler;

        async void Start() {
            await EntityManager.Instance.WaitTillInitialized();
            _ = EntityManager.Instance
                .GetEntityOfType(EntityType.Player, out var targetEntityUnregisteredChannel).transform;

            _handler = LoadGameOverScene;
            targetEntityUnregisteredChannel.RegisterListener(_handler);
        }

        public void LoadGameOverScene() {
            var sceneLoader = SceneLoader.Instance;
            if(sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }
            
            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.GameOver));
        }
        public void LoadMainMenuScene() {
            var sceneLoader = SceneLoader.Instance;
            if(sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector", transform);
                return;
            }

            sceneLoader.StartCoroutine(sceneLoader.LoadSceneAsync(SceneData.EMenuType.Main));
        }
    }
}
