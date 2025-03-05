using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions {
    public class Instantiator : MonoBehaviour {
        [SerializeField] Instantiable[] instantiators;
        [SerializeField] bool instantiateOnStart;

        void Start() {
            if (instantiateOnStart) {
                InstantiateObjects();
            }
        }

        public void InstantiateObjects() {
            foreach (var instantiator in instantiators) {
                instantiator.InstantiateObject();
            }
        }

        [Serializable]
        public class Instantiable {
            [Required] public GameObject prefab;
            [Required] [Tooltip("Should never be null, because we copy the parents position and rotation on start")]
            public Transform parent;
            public bool detatchFromParent;
            public Vector3 spawnLocalPosition;
            public Vector3 spawnLocalRotation;
            
            public void InstantiateObject() {
                var obj = Instantiate(prefab, parent.position, parent.rotation, parent);
                
                // change local position and rotation
                obj.transform.localPosition = spawnLocalPosition;
                obj.transform.localEulerAngles = spawnLocalRotation;
                
                if (detatchFromParent) {
                    obj.transform.parent = null;
                }
            }
        }
    }
}
