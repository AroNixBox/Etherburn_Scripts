using UnityEngine;
using UnityEngine.Events;

namespace Game.Save {
    public class ObjectStateController : MonoBehaviour {
        [SerializeField] UnityEvent<bool> onLoadState;
        void Start() {
            // Check if the object is already saved
            var saveManager = SaveManager.Instance;
            if (saveManager == null) {
                Debug.LogError("SaveManager not found");
                return;
            }
            
            var objectState = saveManager.GetObjectState(gameObject.name);
            if (objectState == null) {
                // Object is not saved yet
                saveManager.RegisterObject(gameObject.name, false);
            } else {
                // Object is saved
                // Use the saved state
                onLoadState.Invoke(objectState.Value);
            }
        }
        
        public void SaveObjectState(bool objectState) {
            // Check if the object is already saved
            var saveManager = SaveManager.Instance;
            if (saveManager == null) {
                Debug.LogError("SaveManager not found");
                return;
            }
            
            saveManager.RegisterObject(gameObject.name, objectState);
        }
    }
}
