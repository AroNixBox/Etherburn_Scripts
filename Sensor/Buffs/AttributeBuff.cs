using System;
using Drawing;
using Extensions;
using Interfaces.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Sensor.Buffs {
    public class AttributeBuff : TriggerArea {
        [SerializeField] BuffTarget buffTarget;
        [SerializeField] float buffAmount;
        [SerializeField] bool multiApply;
        [SerializeField] [ShowIf("@multiApply")] [Range(1, 10)] uint applyCount;
        int _appliedCount;
        bool _hasApplied;

        protected override void FireSpecificAction(Entity entity) {
            if (multiApply) {
                if (_appliedCount >= applyCount) return;
                _appliedCount++;
            } else {
                if (_hasApplied) return;
                _hasApplied = true;
            }

            switch (buffTarget) {
                case BuffTarget.None:
                    return;
                case BuffTarget.Health when entity.TryGetComponent(out IHealth health):
                    health.Increase(buffAmount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        enum BuffTarget {
            None,
            Health,
        }
    }
}
