using Enemy;
using Extensions.FSM;
using Interfaces.Attribute;
using Player.Animation.MotionWarp;
using Player.Weapon;
using Sensor;
using UnityEngine;

namespace Player.States {
    /* @ Exit Condition
     * 
     * Every Attack Exectuion Animation needs to fire the ExecutionEnd Event.
     * The Setup for the Warp is done in SO: WarpAnimation Logic.
     */
    public class AttackUltimateState : IState {
        #region Cached References

        readonly References _references;
        readonly WeaponManager _weaponManager;
        readonly Mover _mover;
        readonly Animation.AnimationController _animationController;
        readonly RootMotionWarpingController _rootMotionWarpingController;
        readonly IEnergy _stamina;
        readonly IEnergy _ultimate;
        
        #endregion

        #region Dynaimc References

        WeaponSO _currentWeapon;
        WarpAnimation _weaponExecution;
        FirstTriggerHitSensor _weaponHitSensor;

        #endregion
        public AttackUltimateState(References references) {
            _references = references;
            _mover = _references.mover;
            _animationController = _references.animationController;
            _weaponManager = _references.weaponManager;
            _rootMotionWarpingController = _mover.RootMotionWarpingControllerController;
            _stamina = _references.StaminaAttribute;
            _ultimate = _references.UltimateAttribute;
        }
        public void OnEnter() { 
            // Data Setup
            _weaponExecution = _weaponManager.GetCurrentFinisher();
            
            PlayAnimation();
            ConsumeAttributes();

            // Warp requires Kinematic RigidBody
            _mover.SetKinematic(true);
            
            var enemy = _references.abilityTargetQuery.GetWarpTargetProvider(EntityType.Enemy);
            if(!enemy.TryGetComponent(out EnemyTargetProvider enemyTargetProvider)) {
                Debug.LogError("Enemy does not have a Target Provider attached to it.");
                return;
            }

            _rootMotionWarpingController.SetWarpAnimationAndTarget(
                enemyTargetProvider.ProvideWarpTarget(_references.transform, false).GetTransform(),
                _weaponExecution, _references.transform.position);
            
            SetupWeaponCollision();

            // Not Collision Based, thrown by Animation (Swash FX & Sound)
            _references.SpawnParticles += SpawnParticles;
            _references.SpawnParticles += PlaySound;
        }
        void PlayAnimation() {
            // Get the seleted weapon
            _currentWeapon = _weaponManager.GetSelectedWeapon();
            
            _weaponManager.ReplaceFinisherFromOverrideController(_weaponExecution.clip);
            
            // Trigger Transition
            _animationController.ChangeAnimationState(Animation.AnimationParameters.AttackFinisher, 
                Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.AttackFinisher), 
                0);
        }
        void ConsumeAttributes() {
            // Consume Stamina
            var staminaCost = _currentWeapon.finisherData.attributeData.stamina;
            _stamina.Increase(staminaCost);
        }
        void SetupWeaponCollision() {
            // If the Weapon has Collision
            _weaponHitSensor = _weaponManager.CurrentWeaponSensor;
            // If its a Meele Weapon with Damage on Collision
            if (_weaponHitSensor is PlayerMeeleWeaponSensor meeleWeaponHitSensor) {
                var attackDamage = _currentWeapon.finisherData.attributeData.damage;
                var ultAttributeGain = _currentWeapon.finisherData.attributeData.ultimate;
                meeleWeaponHitSensor.InitializeSensor(attackDamage, true, ultAttributeGain, _ultimate);
            }
                
            _references.EnableHitDetection += EnableHitDetection;
            _references.DisableHitDetection += DisableHitDetection;
        }
        

        #region Anim Event Method Calls
        // Particles & Sound on Player (Swash FX & Sound)
        void SpawnParticles() {
            // Spawn Particle Swash FX:
            var particleSystem = _weaponExecution.effectInstance.particleSystem;
            var particleInstance = Object.Instantiate(particleSystem, _references.vfxSpawnPointRight);
            particleInstance.transform.SetLocalPositionAndRotation(
                _weaponExecution.effectInstance.spawnPosition, 
                Quaternion.Euler(_weaponExecution.effectInstance.spawnRotation));
            particleInstance.transform.parent = null;
        }
        void PlaySound() {
            _references.weapon2DSource.PlayOneShot(_weaponExecution.effectInstance.spawnSound);
        }
        // Called from the Clip (From - To) contact reading
        void EnableHitDetection() {
            _weaponHitSensor.SetColliderEnabled(true);
        }
        void DisableHitDetection() {
            _weaponHitSensor.SetColliderEnabled(false);
        }

        #endregion

        public void Tick() { }

        public void FixedTick() { }

        public void OnExit() {
            // In case Any Transition is triggered before the Hit Detection is disabled
            DisableHitDetection();
            
            // Reset Warp Conditions
            _mover.SetKinematic(false);
            _rootMotionWarpingController.NullAllConditions();
            
            _weaponManager.IncreaseAttackIndex();
            
            // Reset State Conditions
            // Reset Animation Event Bool Flag for the next Execution
            _references.ExecutionEnded = false;
            
            // Event Unsubscription
            _references.SpawnParticles -= SpawnParticles;
            _references.EnableHitDetection -= EnableHitDetection;
            _references.DisableHitDetection -= DisableHitDetection;
            _references.SpawnParticles -= PlaySound;
        }
    }
}