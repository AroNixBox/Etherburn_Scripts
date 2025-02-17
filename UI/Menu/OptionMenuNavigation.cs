using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    public class OptionMenuNavigation : MonoBehaviour {
        [Header("User Interface")]
        [Title("Buttons")]
        [SerializeField, Required] Button systemButton;
        [SerializeField, Required] Button controlsButton;

        [Title("Panels")]
        [Required] public BaseMenuNavigation mainOptionsPanel;
        [Required] public BaseMenuNavigation systemPanel;
        [Required] public BaseMenuNavigation controlsPanel;

        void Start() {
            SetupButtonNavigation();
        }

        void SetupButtonNavigation() {
            systemButton.onClick.AddListener(() => SwitchPanel(systemPanel));
            controlsButton.onClick.AddListener(() => SwitchPanel(controlsPanel));
        }

        void SwitchPanel(BaseMenuNavigation switchPanel) {
            switchPanel.gameObject.SetActive(true);
            mainOptionsPanel.gameObject.SetActive(false);
        }
    }
}