using System;
using Player.Audio;
using UnityEngine;

namespace Effects {
    [Serializable]
    public class MaterialEffectPair<T> {
        public PhysicsMaterial material;
        public T[] associatedEffects;
    }
}