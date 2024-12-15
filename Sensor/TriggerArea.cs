using System.Collections.Generic;
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
        List<Entity> _targetEntities;
        
        [SerializeField] bool useEventChannel = true;
        EntityColliderInteractionChannel _entityColliderInteractionChannel;
        bool _isInitialized;
        
        [ShowIf("@!useEventChannel")]
        public UnityEvent onCollisionEvent;
        void Start() {
            _collider = GetComponent<Collider>();
            
            // TODO: Find a way to get the target entity without using FindObjectsByType
            var entities = FindObjectsByType<Entity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _targetEntities = entities.Where(entity => entity.EntityType == targetEntityType).ToList();

            if(_targetEntities.Count == 0) {
                Debug.LogError($"No entity of type {targetEntityType} found.");
                return;
            }

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
            if(!other.TryGetComponent(out Entity entity)) { return; }
            if(!_targetEntities.Contains(entity)) { return; }
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    // If the other Object is Kinematic, check for intersection first
                    if (IsColliderIntersecting(other)) {
                        FireEvent();
                        FireSpecificAction(entity);
                    }
                    return;
                }
            }
            
            if (IsColliderIntersecting(other)) {
                FireEvent();
                FireSpecificAction(entity);
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
        protected virtual void FireSpecificAction(Entity entity) { }
        void OnTriggerExit(Collider other) {
            if(!_isInitialized) { return; }
            if (messageType != EMessageType.Exit) { return; }
            if(!other.TryGetComponent(out Entity entity)) { return; }
            if(!_targetEntities.Contains(entity)) { return; }
            
            if(other.TryGetComponent(out Rigidbody rigidbody)) {
                if (rigidbody.isKinematic) {
                    if (!IsColliderIntersecting(other)) {
                        FireEvent();
                        FireSpecificAction(entity);
                    }
                    return;
                }
            }
            // Doublecheck if is really outside of the collider
            if (!IsColliderIntersecting(other)) {
                FireEvent();
                FireSpecificAction(entity);
            }
        }
    
        public enum EMessageType {
            Enter,
            Exit
        }
    }
}