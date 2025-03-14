using UnityEngine;

namespace Extensions {
    public class DetachFromParent : MonoBehaviour {
        [SerializeField] bool detachFromParentOnStart = true;
        void Start() {
            if (detachFromParentOnStart) {
                Detach();
            }
        }

        public void Detach() {
            transform.SetParent(null);
        }
    }
}
