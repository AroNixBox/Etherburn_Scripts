using System.Collections.Generic;
using UnityEngine;

namespace Behavior.Enemy.State.Animation {
    public static class AnimationsParams {
        // State Names
        static readonly int GroundLocomotion = Animator.StringToHash("GroundLocomotion");
        static readonly int AttackA = Animator.StringToHash("AttackA");
        static readonly int AttackB = Animator.StringToHash("AttackB");
        static readonly int Hurt = Animator.StringToHash("Hurt");
        static readonly int Die = Animator.StringToHash("Die");
        static readonly int Eat = Animator.StringToHash("Eat");
        static readonly int Empty = Animator.StringToHash("Empty");
        
        // Blend Tree Values
        // public static readonly float SpeedMagnitude = Animator.StringToHash("SpeedMagnitude");
        
        // Animation Durations
        static readonly Dictionary<AnimationStates, AnimationDetails> AnimationConditions = new() {
            {AnimationStates.GroundLocomotion, new AnimationDetails {StateName = GroundLocomotion, BlendDuration = 0.125f}},
            {AnimationStates.AttackA, new AnimationDetails {StateName = AttackA, BlendDuration = 0f}},
            {AnimationStates.AttackB, new AnimationDetails {StateName = AttackB, BlendDuration = 0f}},
            {AnimationStates.Hurt, new AnimationDetails {StateName = Hurt, BlendDuration = 0.1f}},
            {AnimationStates.Die, new AnimationDetails {StateName = Die, BlendDuration = 0.1f}},
            {AnimationStates.Eat, new AnimationDetails {StateName = Eat, BlendDuration = 0.1f}}
        };
        
        static readonly AnimationDetails EmptyDetails = new() { StateName = Empty, BlendDuration = 0.1f };
        public static AnimationDetails GetAnimationDetails(AnimationStates state) {
            return AnimationConditions.GetValueOrDefault(state, EmptyDetails);
        }

        public class AnimationDetails {
            public int StateName;
            public float BlendDuration;
        }
    }
}