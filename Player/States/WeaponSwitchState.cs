using Extensions.FSM;
using Player.Weapon;
using UnityEngine;

namespace Player.States {
    public class WeaponSwitchState : IState {
        // Initial References
        readonly References _references;
        readonly WeaponManager _weaponManager;
        readonly UI.RadialSelection _radialSelection;
        readonly Animation.AnimationController _animationController;

        public WeaponSwitchState(References references) {
            _references = references;
            _weaponManager = _references.weaponManager;
            _radialSelection = _references.radialSelection;
            _animationController = _references.animationController;
        }
       public void OnEnter() {
           _references.OnMaterializeWeapon += MaterializeNewWeapon;
           _references.OnDissolveWeapon += DissolveOldWeapon;
           
            PlayAnimation();
       }

       void MaterializeNewWeapon() {
           // Read the Radial Selection selected Weapon Index
           var selectedWeaponIndex = _radialSelection.GetSelectedIndex();
           // Pass it to the Weapon Manager, it will set the Weapon
           _weaponManager.SetSelectedWeapon(selectedWeaponIndex);
           
           // Get the new selected Weapon
           var selectedWeapon = _weaponManager.GetSelectedWeapon();
           
           // Reset the Attack Index for the new Weapon
           _weaponManager.ResetAttackIndex();
           // Replace the Animator Controller
           _animationController.OverrideAnimatorController(selectedWeapon.animatorOverrideController);
           
           // Change the Animation Speed Multiplier for Ground Locomotion
           var newGroundSpeedMultiplier = selectedWeapon.groundLocomotionSpeedMultiplier;
           _animationController.ChangeAnimationClipSpeed(Animation.AnimationParameters.GroundLocomotionSpeedMultiplier, newGroundSpeedMultiplier);
           
           // Reset the Weapon Parents Position and Rotation if there was any
           _references.weaponSocket.localPosition = _references.InitialWeaponSocketPosition;
           _references.weaponSocket.localRotation = _references.InitialWeaponSocketRotation;

           // Spawn the new weapon
           var spawnedWeapon = Object.Instantiate(selectedWeapon.weaponPrefab, _references.weaponSocket);
           spawnedWeapon.transform.localPosition = selectedWeapon.weaponOffset;
           spawnedWeapon.transform.localRotation = Quaternion.Euler(selectedWeapon.weaponRotation);

           _weaponManager.CurrentSpawnedWeaponDissolver = spawnedWeapon.GetComponent<ShaderControl.DissolveControl>();
           _weaponManager.CurrentWeaponSensor = spawnedWeapon.GetComponent<Sensor.PlayerMeeleWeaponSensor>();
           
           _ = _weaponManager.CurrentSpawnedWeaponDissolver.ChangeDissolveMode(ShaderControl.DissolveControl.DissolveMode.Materialize);
       }

       async void DissolveOldWeapon() {
           if (_weaponManager.CurrentSpawnedWeaponDissolver != null) {
               var oldWeapon = _weaponManager.CurrentSpawnedWeaponDissolver;
               
               // Null in References
               _weaponManager.CurrentSpawnedWeaponDissolver = null;
               _weaponManager.CurrentWeaponSensor = null;
               
               
               await oldWeapon.ChangeDissolveMode(ShaderControl.DissolveControl.DissolveMode.Dissolve);
               // Destroy the old weapon
               if (oldWeapon != null && oldWeapon.gameObject != null) {
                   Object.Destroy(oldWeapon.gameObject);
               }
           }
       }
       void PlayAnimation() {
           _animationController.ChangeAnimationState(Animation.AnimationParameters.ChangeWeapon, 
               Animation.AnimationParameters.GetAnimationDuration(Animation.AnimationParameters.ChangeWeapon), 
               0);
       } 
       public void Tick() { } 
       public void FixedTick() { }

       public void OnExit() {
           // Fallback, Materialize wasnt triggered:
           if(_weaponManager.CurrentSpawnedWeaponDissolver == null || _weaponManager.CurrentWeaponSensor == null) {
               MaterializeNewWeapon();
           }
           
           _references.OnMaterializeWeapon -= MaterializeNewWeapon;
           _references.OnDissolveWeapon -= DissolveOldWeapon;
           
           _references.ChangeWeaponEnded = false;
       }
    }
}