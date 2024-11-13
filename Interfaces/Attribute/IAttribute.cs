using System;
using UnityEngine;

namespace Interfaces.Attribute {
    public interface IAttribute {
        public event Action<float> OnAttributeValueIncreased;
        public event Action<float> OnAttributeValueDecresed;
        public void Increase(float amount);
        // hitDirection is used to apply knockback
        public void Decrease(float amount, Vector3? hitPosition = null);
    }
}