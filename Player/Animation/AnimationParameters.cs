using System.Collections.Generic;
using UnityEngine;

namespace Player.Animation {
    public static class AnimationParameters {
        // Actually controls the movementSpeed, because 0, 0 is idle in run and walk
        public static readonly int VelocityX = Animator.StringToHash("VelocityX");
        public static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
        // Idle, Walk, Run
        public static readonly int Speed = Animator.StringToHash("Speed");
        // Get Hit
        public static readonly int HitDirectionX = Animator.StringToHash("HitDirectionX");
        public static readonly int HitDirectionZ = Animator.StringToHash("HitDirectionZ");
        // Speed Multipliers
        public static readonly int GroundLocomotionSpeedMultiplier = Animator.StringToHash("GroundLocomotionSpeedMultiplier");
        
        public static readonly int GroundLocomotion = Animator.StringToHash("GroundLocomotion");
        public static readonly int Fall = Animator.StringToHash("Fall");
        public static readonly int Land = Animator.StringToHash("Land");
        public static readonly int Dodge = Animator.StringToHash("Dodge");
        public static readonly int ChangeWeapon = Animator.StringToHash("ChangeWeapon");
        public static readonly int AttackFinisher = Animator.StringToHash("AttackFinisher");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Attack2 = Animator.StringToHash("Attack2");
        public static readonly int GetHit = Animator.StringToHash("Get Hit");
        public static readonly int Die = Animator.StringToHash("Die");
        public static readonly int Reincarnation = Animator.StringToHash("Reincarnation");
        
        static readonly Dictionary<int, float> AnimationDurations = new (){
            // .1f Seconds for fixing "snap", because crossFade sometimes lets one animation end
            // when the other starts, the ended one snaps into the startPos of the new one
            {GroundLocomotion, 0.125f},
            {Fall, 0.25f},
            {Land, 0.25f},
            {Dodge, 0.125f},
            {ChangeWeapon, 0.05f},
            {AttackFinisher, 0.1f},
            {Attack, 0.1f},
            {Attack2, 0.1f},
            {GetHit, 0.1f},
            {Die, 0.1f},
            {Reincarnation, 0f}
        };
        
        public static float GetAnimationDuration(int animationHash) {
            return AnimationDurations.GetValueOrDefault(animationHash, 0.25f);
        }
    }
}