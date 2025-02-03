using Sirenix.OdinInspector;
using UnityEngine;

namespace UI {
    public class UIOnscreenFocus : MonoBehaviour {
        [Title("References")]
        [SerializeField, Required] RectTransform uiElement; 
        [SerializeField, Required]  Camera mainCamera;
        
        Transform _target;

        void Update() {
            if (uiElement == null || mainCamera == null) { return; }

            if (_target == null) {
                uiElement.gameObject.SetActive(false);
                return;
            }

            var screenPos = mainCamera.WorldToScreenPoint(_target.position);
            
            if (screenPos.z > 0) {
                uiElement.position = screenPos;
            } else {
                uiElement.gameObject.SetActive(false);
            }
        }
        
        public void SetTarget(Transform target) {
            _target = target;
            uiElement.gameObject.SetActive(true);
        }
        public void RemoveTarget() {
            _target = null;
            uiElement.gameObject.SetActive(false);
        }
    }
}
