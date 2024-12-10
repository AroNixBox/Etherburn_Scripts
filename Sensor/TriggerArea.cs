using System.Linq;
using Behavior.Events.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Sensor {
    [RequireComponent(typeof(Collider))]
    public class TriggerArea : MonoBehaviour, IRequireEntityColliderInteractionChannel {
        [SerializeField] EntityType targetEntityType;
        [SerializeField] EMessageType messageType;
        Collider _collider;
        Transform _target;
        
        [SerializeField] bool useEventChannel = true;
        EntityColliderInteractionChannel _entityColliderInteractionChannel;
        bool _isInitialized;
        
        [ShowIf("@!useEventChannel")]
        public UnityEvent onCollisionEvent;
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

            if (!useEventChannel) {
                _isInitialized = true;
            }
        }

        public EntityColliderInteractionChannel AssignEventChannel(EntityColliderInteractionChannel entityColliderInteractionChannel) {
            if (!useEventChannel) {
                Debug.LogError("We are not using Event Channel via Bootstrapper");
                return null;
            }
            
            _isInitialized = true;
            if (_entityColliderInteractionChannel == null) {
                _entityColliderInteractionChannel = entityColliderInteractionChannel;
                return _entityColliderInteractionChannel;
            }
            
            // Channel was already assigned, by a different AssignEventChannel call
            return _entityColliderInteractionChannel;
        }
        void OnTriggerEnter(Collider other) {
            if(!_isInitialized) { return; }
            if (messageType != EMessageType.Enter) { return; }
            if(other.transform != _target) { return; }
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    // If the other Object is Kinematic, check for intersection first
                    if (IsColliderIntersecting(other)) {
                        FireEvent();
                    }
                    return;
                }
            }
            
            if (IsColliderIntersecting(other)) {
                FireEvent();
            }
        }
        
        bool IsColliderIntersecting(Collider other) {
            return _collider.bounds.Intersects(other.bounds);
        }

        void FireEvent() {
            if (useEventChannel) {
                _entityColliderInteractionChannel.SendEventMessage();
            }
            else {
                onCollisionEvent?.Invoke();
            }
        }
        void OnTriggerExit(Collider other) {
            if(!_isInitialized) { return; }
            if (messageType != EMessageType.Exit) { return; }
            if(other.transform != _target) { return; }
            
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    if (!IsColliderIntersecting(other)) {
                        FireEvent();
                    }
                    return;
                }
            }
            // Doublecheck if is really outside of the collider
            if (!IsColliderIntersecting(other)) {
                FireEvent();
            }
        }
    
        public enum EMessageType {
            Enter,
            Exit
        }
    }
}