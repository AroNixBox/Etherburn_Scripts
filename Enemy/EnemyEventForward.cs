using System.Collections.Generic;
using System.Linq;
using Behavior.Events.Interfaces;
using Extensions.Animation;
using Sensor;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace Enemy {
    [RequireComponent(typeof(Animator))]
    public class EnemyEventForward : MonoBehaviour, IRequireAttackRotationStoppedChannel {
        [SerializeField, Required] EnemyMover enemyMover;
        [SerializeField, Required] BehaviorGraphAgent behaviorGraphAgent;
        [SerializeField] string weaponsBbvName = "Weapons";
        [SerializeField] List<AnimationEventAction> specialAnimEvents = new();
        DamageDealingObject[] _weapons;
        EnemyAttackRotateStopped _attackRotationStoppedChannel;
        Animator _animator;

        void Awake() {
            _animator = GetComponent<Animator>();
            
            // TODO: Dont do this here!
            _animator.applyRootMotion = true;
        }

        void Start() {
            // Get the weapons from the behavior graph agent
            if (!behaviorGraphAgent.BlackboardReference.GetVariableValue(weaponsBbvName, out List<GameObject> possibleWeapons)) {
                Debug.LogError($"Blackboard variable: {weaponsBbvName} could not be set, the variable name is incorrect or the variable does not exist in the blackboard");
            }
            // Cast the weapons from behavior graph to DamageDealingObject
            _weapons = possibleWeapons.Select(weapon => weapon.GetComponent<DamageDealingObject>()).ToArray();
            
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
            if(_weapons.Length == 0) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]

            foreach (var weapon in _weapons) {
                weapon.CastForObjects(true);
            }
        }
        
        void DisableHitDetection(AnimationEvent evt) {
            if (IsInAnimationTransition(evt)) { return; }
            if(_weapons.Length == 0) { return; } // [WeaponController.cs] will be null when Enemy dies, all Components get destroyed except [EnemyEventForward.cs] and [Animator]

            foreach (var weapon in _weapons) {
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
    }
}
