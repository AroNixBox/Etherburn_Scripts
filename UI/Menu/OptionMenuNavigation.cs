using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu {
    public class OptionMenuNavigation : MonoBehaviour {
        [Header("User Interface")]
        [Title("Buttons")]
        [SerializeField, Required] Button systemButton;
        [SerializeField, Required] Button controlsButton;

        [Title("Panels")]
        [Required] public GameObject mainOptionsPanel;
        [Required] public GameObject systemPanel;
        [Required] public GameObject controlsPanel;

        void Start() {
            SetupButtonNavigation();
        }

        void SetupButtonNavigation() {
            systemButton.onClick.AddListener(() => SwitchPanel(systemPanel));
            controlsButton.onClick.AddListener(() => SwitchPanel(controlsPanel));
        }

        void SwitchPanel(GameObject p0) {
            p0.SetActive(true);
            mainOptionsPanel.SetActive(false);
        }

        public bool IsAnyPanelActive() {
            return systemPanel.activeSelf || controlsPanel.activeSelf || mainOptionsPanel.activeSelf;
        }
    }
}