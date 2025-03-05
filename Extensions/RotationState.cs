using UnityEngine;
using UnityEngine.Events;

namespace Extensions {
    public class RotationState : MonoBehaviour {
        [SerializeField] Vector3 endRotation = new(0, 90, 0);
        [SerializeField] UnityEvent rotationStateChange;

        public void TriggerRotation(bool opened) {
            if(!opened) { return; }
            
            transform.Rotate(endRotation);
            rotationStateChange?.Invoke();
        }
    }
}
