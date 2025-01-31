using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menu {
    public class RebindElement : MonoBehaviour {
        [Title("References")]
        [SerializeField] RebindHandler rebindHandler;
        [SerializeField] InputActionReference inputActionReference;
        [Title("User Interface")]
        [SerializeField] TMP_Text actionNameText;
        [SerializeField] Image buttonBackground;
        
        string _inputActionName;
        void Start() {
            _inputActionName = inputActionReference.action.name;
            actionNameText.text = rebindHandler.GetActionName(_inputActionName);
            
            var rebindButtonChildren = GetComponentsInChildren<RebindButton>();
            foreach (var rebindButton in rebindButtonChildren) {
                rebindButton.Initialize(inputActionReference, rebindHandler);
            }
        }
    }
}
