using System;
using Extensions;
using Interfaces.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sensor.Buffs {
    public class AttributeBuff : FirstTriggerHitSensor {
        [SerializeField] float buffAmount;
        [SerializeField] BuffTarget buffTarget;
        [SerializeField] bool multiApply;
        [SerializeField] [ShowIf("@multiApply")] [Range(1, 10)] uint applyCount;
        int _appliedCount;

        protected override void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponentInParent(out IAttribute attribute)) return;
        
            switch (buffTarget) {
                case BuffTarget.Health:
                    if (other.TryGetComponentInParent(out IHealth health)) {
                        health.Increase(buffAmount);
                    }
                    break;
                case BuffTarget.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            attribute.Increase(buffAmount);
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (((1 << other.gameObject.layer) & excludedLayers) != 0) {
                return;
            }
        
            collisionEvent?.Invoke(other.transform, null, other.transform.position, other.transform.up);

            if (!multiApply) {
                SetColliderEnabled(false);
                return;
            }
        
            if (_appliedCount >= applyCount) {
                SetColliderEnabled(false);
                return;
            }
            _appliedCount++;
        }

        public enum BuffTarget {
            None,
            Health,
        }
    }
}
