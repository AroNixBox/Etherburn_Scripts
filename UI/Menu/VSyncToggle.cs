using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(Toggle))]
    public class VSyncToggle : MonoBehaviour {
        Toggle _vSyncToggle;

        void Awake() {
            _vSyncToggle = GetComponent<Toggle>();
        }

        void Start() {
            // Initialize the toggle state based on the current VSync setting
            _vSyncToggle.isOn = QualitySettings.vSyncCount > 0;

            // Add listener to handle toggle changes
            _vSyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
        }

        void OnVSyncToggleChanged(bool isOn) {
            // Enable or disable VSync based on the toggle state
            QualitySettings.vSyncCount = isOn ? 1 : 0;
        }
    }
}