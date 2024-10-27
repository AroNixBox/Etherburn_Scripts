using System;
using UnityEngine;

namespace Interfaces.Attribute {
    // Use to make an entity killable
    public interface IHealth : IAttribute {
        public Vector3 HitPosition { get; }
        public bool HasTakenDamage { get; set;  }
        public bool HasDied { get; set; }
    }
}