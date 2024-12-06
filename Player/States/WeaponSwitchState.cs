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
        bool _switchCompleted;

        public WeaponSwitchState(References references) {
            _references = references;
            _weaponManager = _references.weaponManager;
            _radialSelection = _references.radialSelection;
            _animationController = _references.animationController;
        }
       public void OnEnter() {
            SwitchWeapon();
       }
       async void SwitchWeapon() {
           PlayAnimation();
            
           // Trigger dissolve on old weapon
           if (_weaponManager.CurrentSpawnedWeaponDissolver != null) {
               bool dissolveCompleted = await _weaponManager.CurrentSpawnedWeaponDissolver.ChangeDissolveMode(ShaderControl.DissolveControl.DissolveMode.Dissolve);
               if (!dissolveCompleted) {
                   Debug.LogError("Failed to dissolve the old weapon.");
                   return;
               }
               // Destroy the old weapon
               if (_weaponManager.CurrentSpawnedWeaponDissolver != null &&
                   _weaponManager.CurrentSpawnedWeaponDissolver.gameObject != null) {
                   Object.Destroy(_weaponManager.CurrentSpawnedWeaponDissolver.gameObject);
               }
           }
            
           // Read the Radial Selection selected Weapon Index
           var selectedWeaponIndex = _radialSelection.GetSelectedIndex();
           // Pass it to the Weapon Manager, it will set the Weapon
           _weaponManager.SetSelectedWeapon(selectedWeaponIndex);
            
           // Get the new selected Weapon
           var selectedWeapon = _weaponManager.GetSelectedWeapon();
           // Spawn the new weapon
           var spawnedWeapon = Object.Instantiate(selectedWeapon.weaponPrefab, _references.weaponSocket);
           spawnedWeapon.transform.localPosition = selectedWeapon.weaponOffset;
           spawnedWeapon.transform.localRotation = Quaternion.Euler(selectedWeapon.weaponRotation);
           _weaponManager.SetSpawnedWeaponData(
               spawnedWeapon.GetComponent<ShaderControl.DissolveControl>(), 
               spawnedWeapon.GetComponent<Sensor.PlayerMeeleWeaponSensor>());
           // Set the Anim Controller
           _animationController.OverrideAnimatorController(selectedWeapon.animatorOverrideController);
           await _weaponManager.CurrentSpawnedWeaponDissolver.ChangeDissolveMode(ShaderControl.DissolveControl.DissolveMode.Materialize);
           
           _switchCompleted = true;
       }
       public bool SwitchCompleted() {
           return _switchCompleted;
       }
       void PlayAnimation() {
            // TODO:
       } 
       public void Tick() { } 
       public void FixedTick() { }

       public void OnExit() {
           _switchCompleted = false;
       }
    }
}