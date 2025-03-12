using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Extensions.Animation {
    [Serializable]
    public class AnimationEventAction {
        [Tooltip("List of special event names that can be triggered during animations." +
                 "Make sure to add the event name in the Animation Event in the Animation Clip")]
        public string eventName;
        public UnityEvent action;
        public bool instantiator;
        [Required]
        [ShowIf("@instantiator")]
        public GameObject prefab;
        [Required]
        [ShowIf("@instantiator")]
        [Tooltip("Should never be null, because we copy the parents position and rotation on start")]
        public Transform parent;
        [ShowIf("@instantiator")]
        public bool detatchFromParent;
        [ShowIf("@instantiator")]
        public Vector3 spawnLocalPosition;
        [ShowIf("@instantiator")]
        public Vector3 spawnLocalRotation;
        
        public void InstantiateObject() {
            var obj = Object.Instantiate(prefab, parent.position, parent.rotation, parent);
            if(parent.gameObject != null && obj.scene != parent.gameObject.scene) {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, parent.gameObject.scene);
            }
            
            // change local position and rotation
            obj.transform.localPosition = spawnLocalPosition;
            obj.transform.localEulerAngles = spawnLocalRotation;
            
            if (detatchFromParent) {
                obj.transform.parent = null;
            }
        }
    }
}