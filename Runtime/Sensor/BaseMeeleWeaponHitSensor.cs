using Extensions;
using Interfaces.Attribute;
using UnityEngine;

namespace Sensor {
    public class BaseMeeleWeaponHitSensor : FirstTriggerHitSensor {
        protected float DamageAmount;

        // Initialize Damage Amount only, since this is common
        public virtual void InitializeSensor(float damageAmount) {
            DamageAmount = damageAmount;
        }

        protected override void OnTriggerEnter(Collider other) {
            if (DamageAmount == 0) { Debug.LogError("InitializeSensor() was not called, will deal no damage"); } 
            
            // Base Call is for VFX, SFX, etc.
            base.OnTriggerEnter(other);
            
            if (other.TryGetComponentInParent(out IHealth health)) {
                ApplyHit(other, health);
            }
        }

        protected virtual void ApplyHit(Collider other, IHealth health) {
            var hitPosition = GetClosestPoint(other);
            health.Decrease(DamageAmount, hitPosition);
        }
    }
}