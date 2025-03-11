using Extensions.FSM;
using Interfaces.Attribute;
using Player.Animation;
using Player.Cam;
using Player.Weapon;
using Sensor;
using UnityEngine;

namespace Player.States {
    public class AttackState : IState {
        public enum AttackType { Light, Heavy }

        #region Cached References

        readonly AttackType _attackType;
        readonly References _references;
        readonly WeaponManager _weaponManager;
        readonly Mover _mover;
        readonly OrbitalController _orbitalController;
        readonly IEnergy _stamina;
        readonly IEnergy _ultimate;

        #endregion

        #region Dynamic References

        WeaponSO _currentWeapon;
        PlayerMeeleWeaponSensor _weaponHitSensor;
        AttackData _currentAttackData;
        AnimationEffect _attackAnimationEffect;

        #endregion
        
        public AttackState(References references, AttackType attackType) {
            _references = references;
            _attackType = attackType;
            _weaponManager = _references.weaponManager; 
            _mover = _references.mover;
            _orbitalController = _references.orbitalController;
            _stamina = _references.StaminaAttribute;
            _ultimate = _references.UltimateAttribute;
        }

        public void OnEnter() {
            // If we are locked on a Target, face the Target
            if(_orbitalController.IsLockedOnTarget()) {
                _references.mover.CanApplyModelRotationInCameraForward = true;
            }

            // Data Setup
            _currentWeapon = _weaponManager.GetSelectedWeapon();
            _currentAttackData = _attackType == AttackType.Light 
                ? _currentWeapon.lightAttack 
                : _currentWeapon.heavyAttack;
            
            
            // Physics
            _mover.SetGravity(true);
            
            ConsumeStamina();
            SetupWeaponCollision();
            PlayAttackAnimation();
        }
        void ConsumeStamina() {
            var attackStaminaCost = _currentAttackData.attributeData.stamina;
            _stamina.Decrease(attackStaminaCost);
        }
        void SetupWeaponCollision() {
            // If the Weapon has Collision
            _weaponHitSensor = _weaponManager.CurrentWeaponSensor;
            
            ConfigureDamageSensor(_weaponHitSensor);
                
            _references.EnableHitDetection += EnableHitDetection;
            _references.EnableHitDetection += SpawnParticles;
            _references.EnableHitDetection += PlaySound;
            _references.DisableHitDetection += DisableHitDetection;
        }
        void SpawnParticles() {
            // Spawn Particle Swash FX:
            var particleSystem = _attackAnimationEffect.effect.particleSystem;
            var particleInstance = Object.Instantiate(particleSystem, _references.vfxSpawnPointRight);
            
            if (particleInstance.gameObject.scene != _references.gameObject.scene) {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(particleInstance.gameObject, _references.gameObject.scene);
            }

            // Set the local position and rotation
            particleInstance.transform.localPosition = _attackAnimationEffect.effect.spawnPosition;
            particleInstance.transform.localRotation = Quaternion.Euler(_attackAnimationEffect.effect.spawnRotation);
            
            // Keep highest hierarchy parent to keep positioning. 
            particleInstance.transform.SetParent(_references.transform);
        }
        void PlaySound() {
            _references.weapon2DSource.PlayOneShot(_attackAnimationEffect.effect.spawnSound);
        }
        void ConfigureDamageSensor(PlayerMeeleWeaponSensor meeleWeaponHitSensor) {
            var attackDamage = _currentAttackData.attributeData.damage;
            var ultAttributeGain = _currentAttackData.attributeData.ultimate;

            meeleWeaponHitSensor.InitializeSensor(attackDamage, false, ultAttributeGain, _ultimate); 
        }
        void PlayAttackAnimation() {
            // Get the right Clip
            _attackAnimationEffect = _attackType == AttackType.Light 
                ? _weaponManager.GetCurrentLightAttack() 
                : _weaponManager.GetCurrentHeavyAttack();
            
            // Replace the Attack Clip in the Animator
            var attackClip = _attackAnimationEffect.animationClip;
            _weaponManager.ReplaceAttackFromOverrideController(attackClip, _references.UseFirstAttackClip);
            
            // Get the right Animation State, we use two to chain Attacks together and not wander in default pose due
            // to cant transition to self and reset the Motion State at the same time without it looking weird
            int animAttackState = _references.UseFirstAttackClip 
                ? AnimationParameters.Attack 
                : AnimationParameters.Attack2;
            
            // Play the Animation
            _references.animationController.ChangeAnimationState(animAttackState, 
                AnimationParameters.GetAnimationDuration(animAttackState), 
                0);
        }


        #region Anim Event Method Calls
        // Called from the Animation Evnent on each Light Attack to enable/disable the Hit Detection
        void EnableHitDetection() {
            _weaponHitSensor.CastForObjects(true);
        }
        void DisableHitDetection() {
            _weaponHitSensor.CastForObjects(false);
        }
        #endregion
        
        public void Tick() { }
        public void FixedTick() { }

        public void OnExit() {
            // Dont listen to the Events when we are not in the Attack State anymore
            _references.EnableHitDetection -= EnableHitDetection;
            _references.EnableHitDetection -= SpawnParticles;
            _references.DisableHitDetection -= DisableHitDetection;
            _references.EnableHitDetection -= PlaySound;
            
            // In case Any Transition is triggered before the Hit Detection is disabled
            DisableHitDetection();
            
            // Physics
            _mover.SetGravity(false);
            
            // Switch to the other Attack State
            _references.UseFirstAttackClip = !_references.UseFirstAttackClip;
            
            // We have Attacked, Increase the Index
            _weaponManager.IncreaseAttackIndex();
            
            // Exit Condition Reset
            _references.AttackEnded = false;
            
            // Disable facing the target here because GroundLocomotionState sets this bool individually and
            // AttackState sets it to true On Enter if we are locked on a target, for no other state we need it
            // If so we would set it to true On Enter for that state
            _references.mover.CanApplyModelRotationInCameraForward = false;
        }
    }
}