using UnityEngine;

namespace Extensions {
    public class DetachFromParent : MonoBehaviour {
        void Start() {
            transform.SetParent(null);
        }
    }
}
