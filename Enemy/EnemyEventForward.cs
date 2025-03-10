using System;
using System.Collections.Generic;
using System.Linq;
using Behavior.Events.Interfaces;
using Sensor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Enemy {
    [RequireComponent(typeof(Animator))]
    public class EnemyEventForward : MonoBehaviour, IRequireAttackRotationStoppedChannel {
        [SerializeField, Required] EnemyMover enemyMover;
        [SerializeField, Required] DamageDealingObject[] weapons;
        [SerializeField] List<AnimationEventAction> specialAnimEvents = new();
        EnemyAttackRotateStopped _attackRotationStoppedChannel;
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
            
            // TODO: Dont do this here!
            _animator.applyRootMotion = true;
        }

        void Start() {
            // If Special Event is an Instantiator, then add the InstantiateObject method to the UnityEvent
            foreach (var specialEvent in specialAnimEvents.Where(specialEvent => specialEvent.instantiator)) {
                specialEvent.action.AddListener(specialEvent.InstantiateObject);
            }
        }
        
        public void AssignEventChannel(EnemyAttackRotateStopped attackRotationStoppedChannel) {
            _attackRotationStoppedChannel = attackRotationStoppedChannel;
        }

        // EnemyMover will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]
        void OnAnimatorMove() {
            if(enemyMover == null) { return; }
            
            enemyMover.AnimatorMove(_animator.rootPosition);
        }
        
        void SpecialAnimationAction(string eventName) {
            var matchedEvent = specialAnimEvents.Find(e => e.eventName == eventName);

            matchedEvent?.action?.Invoke();
        }
        
        // Animation Events
        void EnableHitDetection(AnimationEvent evt) {
            if (IsInAnimationTransition(evt)) { return; }
            if(weapons.Length == 0) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]

            foreach (var weapon in weapons) {
                weapon.CastForObjects(true);
            }
        }
        
        void DisableHitDetection(AnimationEvent evt) {
            if (IsInAnimationTransition(evt)) { return; }
            if(weapons.Length == 0) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]

            foreach (var weapon in weapons) {
                weapon.CastForObjects(false);
            }
        }
        
        void StopRotationTowardsPlayer(AnimationEvent evt) {
            _attackRotationStoppedChannel?.SendEventMessage();
        }
        
        bool IsInAnimationTransition(AnimationEvent evt) {
            return evt.animatorClipInfo.weight <= 0.95f;
        }

        void OnDestroy() {
            // If Special Event is an Instantiator, then add the InstantiateObject method to the UnityEvent
            foreach (var specialEvent in specialAnimEvents.Where(specialEvent => specialEvent.instantiator)) {
                specialEvent.action.RemoveListener(specialEvent.InstantiateObject);
            }
        }

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
