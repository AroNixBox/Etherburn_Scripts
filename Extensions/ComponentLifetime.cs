using UnityEngine;

namespace Extensions {
    public class ComponentLifetime : MonoBehaviour {
        [SerializeField] float lifetime = 1f;  
        [SerializeField] MonoBehaviour[] componentsToDisable;
        [SerializeField] bool disableFromEvent;
        
        void Start() {
            if (disableFromEvent) { return; }
            Invoke(nameof(DisableComponents), lifetime);
        }
        
        public void DisableComponents() {
            foreach (var component in componentsToDisable) {
                // detatch the component from the GameObject
                Destroy(component);
            }
             
            Destroy(this);
        }
    }
}