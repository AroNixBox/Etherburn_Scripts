using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(Toggle))]
    public class RunToggleableToggle : MonoBehaviour {
        Toggle _runToggleableToggle;

        void Awake() {
            _runToggleableToggle = GetComponent<Toggle>();
        }

        void Start() {
            var gameBrain = GameBrain.Instance;
            if (gameBrain == null) {
                return;
            }

            // Set the toggle to the current value of RunToggleable
            _runToggleableToggle.isOn = gameBrain.RunToggleable;

            // Add listener to handle toggle changes
            _runToggleableToggle.onValueChanged.AddListener(OnRunToggleableChanged);
        }

        void OnRunToggleableChanged(bool arg0) {
            var gameBrain = GameBrain.Instance;
            if (gameBrain == null) {
                return;
            }

            // Update the RunToggleable property in GameBrain
            gameBrain.RunToggleable = arg0;
        }

        void OnDisable() {
            var gameBrain = GameBrain.Instance;
            if (gameBrain == null) {
                return;
            }
            
            _runToggleableToggle.onValueChanged.RemoveListener(OnRunToggleableChanged);
        }
    }
}