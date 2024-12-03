using System.Linq;
using UnityEngine;

namespace Sensor {
    [RequireComponent(typeof(Collider))]
    public class TriggerExitArea : MonoBehaviour, IRequireEntityColliderInteractionChannel {
        [SerializeField] EntityType targetEntityType;
        [SerializeField] EMessageType messageType;
        Collider _collider;
        Transform _target;
    
        EntityColliderInteractionChannel _entityColliderInteractionChannel;
        bool IsInitialized => _entityColliderInteractionChannel != null;

        void Start() {
            _collider = GetComponent<Collider>();
            
            // TODO: Find a way to get the target entity without using FindObjectsByType
            var entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var targetEntities = entities.Where(entity => entity.EntityType == targetEntityType).ToList();

            if(targetEntities.Count == 0) {
                Debug.LogError($"No entity of type {targetEntityType} found.");
                return;
            }
            // TODO: Find a way to get the target entity without using First(), very weird
            _target = targetEntities.First().transform;
        }

        public void AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel) {
            _entityColliderInteractionChannel = entityColliderInteractionChannel;
        }
        void OnTriggerEnter(Collider other) {
            if(!IsInitialized) { return; }
            if (messageType != EMessageType.Enter) { return; }
            if(other.transform != _target) { return; }
            if(other.attachedRigidbody != null) {
                if (other.attachedRigidbody.isKinematic) { return; }
            }
            
            if (_collider.bounds.Contains(other.transform.position)) { return; }
            
            _entityColliderInteractionChannel.SendEventMessage();
        }
        void OnTriggerExit(Collider other) {
            if(!IsInitialized) { return; }
            if (messageType != EMessageType.Exit) { return; }
            if(other.transform != _target) { return; }
            
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    Debug.Log("Other is Kinematic");
                    return;
                }
            }
            if (_collider.bounds.Contains(other.transform.position)) {
                return;
            }
            // Doublecheck if is really outside of the collider
            _entityColliderInteractionChannel.SendEventMessage();
        }
    
        public enum EMessageType {
            Enter,
            Exit
        }
    }
}