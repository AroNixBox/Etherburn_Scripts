using System;
using Interfaces.Attribute;
using UnityEngine;

namespace Sensor.Buffs {
    public class AttributeBuff : TriggerArea {
        [Header("Attribute Specifics")]
        [SerializeField] BuffTarget buffTarget;
        [SerializeField] BuffType buffType;
        [SerializeField] float buffAmount;

        protected override void FireSpecificAction(Entity entity) {
            switch (buffTarget) {
                case BuffTarget.None:
                    return;
                case BuffTarget.Health when entity.TryGetComponent(out IHealth health):
                    if(buffType == BuffType.Increase) {
                        health.Increase(buffAmount);
                    } else if(buffType == BuffType.Decrease) {
                        var hitPosition = transform.position;
                        health.Decrease(buffAmount, hitPosition);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        enum BuffTarget {
            None,
            Health,
        }
        enum BuffType {
            None,
            Increase,
            Decrease,
        }
    }
}
