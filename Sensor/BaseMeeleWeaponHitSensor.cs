using System.Collections.Generic;
using Extensions;
using Interfaces.Attribute;
using UnityEngine;

namespace Sensor {
    public class BaseMeeleWeaponHitSensor : RaycastInBetweenTransformsSensor {
        float _damageAmount;

        // Initialize Damage Amount only, since this is common
        protected void InitializeSensor(float damageAmount) {
            _damageAmount = damageAmount;
        }

        protected override List<RaycastHit> DetectHitsInSensor() {
            if (_damageAmount == 0) { Debug.LogError("InitializeSensor() was not called, will deal no damage"); }
            
            var hitList = base.DetectHitsInSensor();

            foreach (var hit in hitList) {
                var hitPoint = hit.point;
                if (hit.collider.TryGetComponentInParent(out IHealth health)) {
                    ApplyHit(hitPoint, health);
                }
            }
            
            return hitList;
        }

        protected virtual void ApplyHit(Vector3 hitPoint, IHealth health) {
            health.Decrease(_damageAmount, hitPoint);
        }
    }
}