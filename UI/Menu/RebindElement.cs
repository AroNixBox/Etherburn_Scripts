using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Menu {
    public class RebindElement : MonoBehaviour {
        [Title("References")]
        [SerializeField] RebindHandler rebindHandler;
        [SerializeField] InputActionReference inputActionReference;
        [Title("User Interface")]
        [SerializeField] TMP_Text actionNameText;
        
        void Start() {
            if(rebindHandler == null) {
                Debug.LogError("Rebind Handler is not set");
                return;
            }
            
            actionNameText.text = rebindHandler.GetAction(inputActionReference).name;
            
            var rebindButtonChildren = GetComponentsInChildren<RebindButton>();
            foreach (var rebindButton in rebindButtonChildren) {
                rebindButton.Initialize(inputActionReference, rebindHandler);
            }
        }
    }
}
