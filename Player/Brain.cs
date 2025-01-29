using System;
using Enemy;
using Extensions.FSM;
using Interfaces.Attribute;
using Player.Ability;
using Player.Animation;
using Player.Animation.MotionWarp;
using Player.Input;
using Player.Weapon;
using TMPro;
using UI;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(References))]
    public class Brain : MonoBehaviour {
        [SerializeField] TMP_Text debugText;
        [SerializeField] TMP_Text debugFPS;
        
        StateMachine _stateMachine;
        References _references;
        
        #region Transition Condition References

        RootMotionWarpingController _rootMotionWarpingController;
        AbilityTargetQuery _abilityTargetQuery;
        
        InputReader _input;
        
        Mover _mover;
        WeaponManager _weaponManager;
        IHealth _healthAttribute;
        IEnergy _staminaAttribute;
        IEnergy _ultimateAttribute;
        AnimationController _animationController;
        RadialSelection _radialSelection;

        #endregion

        void Awake() {
            _references = GetComponent<References>();
        }

        void Start() {
            _radialSelection = _references.radialSelection;
            _animationController = _references.animationController;
            
            // Attributes
            _healthAttribute = _references.HealthAttribute;
            _staminaAttribute = _references.StaminaAttribute;
            _ultimateAttribute = _references.UltimateAttribute;
            _weaponManager = _references.weaponManager;
            _mover = _references.mover;
            
            _input = _references.input;
            // Check if Playeractions are enabled
            _input.EnablePlayerActions();
            
            _rootMotionWarpingController = _references.mover.RootMotionWarpingControllerController;
            _abilityTargetQuery = _references.abilityTargetQuery;
            
            SetupStateMachine();
        }

        void SetupStateMachine() {
            _stateMachine = new StateMachine();
            
            // Base Locomotion
            var groundedLocomotion = new States.GroundLocomotionState(_references);
            var falling = new States.FallingState(_references);
            var sliding = new States.SlidingState(_references);
            var landing = new States.LandingState(_references);
            var dodging = new States.DodgingState(_references);
            
            // UI States
            var ui_weaponMenu = new States.WeaponMenuState(_references);
            
            // Weapon
            var weaponSwitchState = new States.WeaponSwitchState(_references);
            
            // Attack
            var attackUltimate = new States.AttackUltimateState(_references);
            var lightAttack = new States.AttackState(_references, States.AttackState.AttackType.Light);
            var heavyAttack = new States.AttackState(_references, States.AttackState.AttackType.Heavy);
            
            // Health
            var getHit = new States.GetHitState(_references);
            var die = new States.DieState(_references, DiscardStateMachine);
            
            // End State Machine
            void DiscardStateMachine() {
                if(debugText != null) {
                    _stateMachine.OnDebugStateChanged -= UpdateDebugState;
                }
                _stateMachine = null;
            }
            
            
            // Grounded Locomotion
            At(groundedLocomotion, falling, () => !_mover.IsGrounded());
            At(groundedLocomotion, sliding, () => _mover.IsGrounded() && _mover.IsGroundTooSteep());
            At(groundedLocomotion, dodging, () => _references.DodgeKeyPressed 
                                                  && _staminaAttribute.HasEnough(DodgeStaminaCost()));
            At(groundedLocomotion, ui_weaponMenu, () => _references.MiddleKeyPressed);
            At(groundedLocomotion, attackUltimate, () => _references.UltimateKeyPressed
                                                          && _ultimateAttribute.HasEnough(UltimateAttributeCost())
                                                          && !_animationController.IsInTransition(0) 
                                                          && IsWarpPossible());
            At(groundedLocomotion, lightAttack, () => _references.AttackKeyPressed
                                                      && _staminaAttribute.HasEnough(LightAttackStaminaCost()) 
                                                      && !_animationController.IsInTransition(0));
            At(groundedLocomotion, heavyAttack, () => _references.SecondAttackKeyPressed
                                                      && _staminaAttribute.HasEnough(HeavyAttackStaminaCost()) 
                                                      && !_animationController.IsInTransition(0));
            
            // Dodging
            // TODO: If we want a Dodging to Dodging, we also need to use two States in the Animator Controller to allow the Crossfade
            At(dodging, groundedLocomotion, () => _references.DodgeEnded 
                                                  && _mover.IsGrounded()
                                                  && !_mover.IsGroundTooSteep());
            At(dodging, falling, () => _references.DodgeEnded
                                       && (!_mover.IsGrounded() || _mover.IsGroundTooSteep()));
            
            // Falling
            At(falling, landing, () => _mover.IsGrounded() && !_mover.IsGroundTooSteep());
            At(falling, sliding, () => _mover.IsGrounded() && _mover.IsGroundTooSteep());
            
            // Sliding
            At(sliding, landing, () => _mover.IsGrounded() && !_mover.IsGroundTooSteep());
            At(sliding, falling, () => !_mover.IsGrounded());
            
            At(landing, groundedLocomotion, () => _references.LandEnded);
            
            
            At(ui_weaponMenu, groundedLocomotion, () => !_references.MiddleKeyPressed 
                                                        && !_weaponManager.HasSelectedNewWeapon(_radialSelection.GetSelectedIndex()));
            At(ui_weaponMenu, weaponSwitchState, () => !_references.MiddleKeyPressed 
                                                  && _weaponManager.HasSelectedNewWeapon(_radialSelection.GetSelectedIndex()));
            
            At(weaponSwitchState, groundedLocomotion, () => _references.ChangeWeaponEnded);
            
            At(attackUltimate, groundedLocomotion, () => _references.ExecutionEnded);
            
            // Light Attack
            At(lightAttack, groundedLocomotion, () => _references.AttackEnded && NoAttackKeyPressed());
            At(lightAttack, lightAttack, () => _references.AttackEnded 
                                               && _staminaAttribute.HasEnough(LightAttackStaminaCost()) 
                                               && _references.AttackKeyPressed);
            At(lightAttack, heavyAttack, () => _references.AttackEnded 
                                               && _staminaAttribute.HasEnough(HeavyAttackStaminaCost()) 
                                               && _references.SecondAttackKeyPressed);

            //Heavy Attack
            At(heavyAttack, groundedLocomotion, () => _references.AttackEnded && NoAttackKeyPressed());
            At(heavyAttack, heavyAttack, () => _references.AttackEnded 
                                               && _references.SecondAttackKeyPressed
                                               && _staminaAttribute.HasEnough(HeavyAttackStaminaCost()));
            At(heavyAttack, lightAttack, () => _references.AttackEnded 
                                               && _references.AttackKeyPressed
                                               && _staminaAttribute.HasEnough(LightAttackStaminaCost()));
            
            Any(getHit, () => _healthAttribute.HasTakenDamage && !_healthAttribute.HasDied);
            Any(die, () => _healthAttribute.HasDied);
            At(getHit, groundedLocomotion, () => _references.GetHitEnded);
            
            if (debugText != null) {
                _stateMachine.OnDebugStateChanged += UpdateDebugState;
            }
            
            _stateMachine.SetInitialState(weaponSwitchState);
            
            return;
            
            void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
            
            bool NoAttackKeyPressed() => !_references.AttackKeyPressed && !_references.SecondAttackKeyPressed;
            
            // Stamina
            float LightAttackStaminaCost() => _weaponManager.GetSelectedWeapon().lightAttack.attributeData.stamina;
            float HeavyAttackStaminaCost() => _weaponManager.GetSelectedWeapon().heavyAttack.attributeData.stamina;
            float DodgeStaminaCost() => _references.dodgeStaminaCost;
            
            // Ultimate Attribute Cost
            float UltimateAttributeCost() => _weaponManager.GetSelectedWeapon().finisherData.attributeData.ultimate;
            
            bool IsWarpPossible(){
                var enemy = _abilityTargetQuery.GetWarpTargetProvider(EntityType.Enemy);
                if(enemy == null) return false;
                
                if(!enemy.TryGetComponent(out EnemyTargetProvider warpTargetProvider)) {
                    Debug.LogError("Enemy does not have a Target Provider attached to it.");
                    return false;
                }

                return _rootMotionWarpingController.IsWarpPossible(
                    warpTargetProvider.ProvideWarpTarget(_references.transform).GetTransform(), 
                    _weaponManager.GetCurrentFinisher(), _references.warpRootMotionMultiplier);
            } 
        }
        void UpdateDebugState(string newStateName) {
            debugText.text = "Current State: " + newStateName;
        }

        void Update() {
            debugFPS.text = "FPS: " + (1.0f / Time.deltaTime).ToString("F2");
            if(_stateMachine == null) return;
            
            _stateMachine.Tick();
        }
        void FixedUpdate() {
            if(_stateMachine == null) return;

            _stateMachine.FixedTick();
        }

        void OnDestroy() {
            if (debugText != null && _stateMachine != null) {
                _stateMachine.OnDebugStateChanged -= UpdateDebugState;
            }
        }
    }
}