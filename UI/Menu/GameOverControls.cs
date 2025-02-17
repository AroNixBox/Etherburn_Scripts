using Game;
using UnityEngine;

namespace UI.Menu {
    [RequireComponent(typeof(BaseMenuNavigation))]
    public class GameOverControls : MonoBehaviour {
        BaseMenuNavigation _baseMenuNavigation;

        void Awake() {
            _baseMenuNavigation = GetComponent<BaseMenuNavigation>();
        }

        void Start() {
            _baseMenuNavigation.onCloseButtonPressed.AddListener(OnCloseButtonPressed);
        }

        void OnCloseButtonPressed() {
            var gameBrain = GameBrain.Instance;
            if (gameBrain == null) {
                Debug.LogError("GameBrain is not in the scene", transform);
                return;
            }
            
            gameBrain.QuitTriggered = true;
        }
        
        void OnDestroy() {
            _baseMenuNavigation.onCloseButtonPressed.RemoveListener(OnCloseButtonPressed);
        }
    }
}
