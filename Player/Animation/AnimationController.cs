using UnityEngine;

namespace Player.Animation {
    /// <summary>
    /// Here all Animation Parameters are set.
    /// OnAnimatorMove is called in {Mover.cs}
    /// </summary>
    public class AnimationController : MonoBehaviour {
        [SerializeField] Animator animator;
        public void ChangeAnimationClipSpeed(int speedMultiplierParam, float newSpeed) {
            animator.SetFloat(speedMultiplierParam, newSpeed);
        }
        public void OverrideAnimatorController(RuntimeAnimatorController controller) {
            animator.runtimeAnimatorController = controller;
        }
        public void UpdateAnimatorSpeed(float speed) {
            animator.SetFloat(AnimationParameters.Speed, speed);
        }
        public void UpdateAnimatorVelocity(Vector2 velocity) {
            animator.SetFloat(AnimationParameters.VelocityX, velocity.x);
            animator.SetFloat(AnimationParameters.VelocityZ, velocity.y);
        }
        public void UpdateAnimatorHitDirection(Vector2 hitDirection) {
            animator.SetFloat(AnimationParameters.HitDirectionX, hitDirection.x);
            animator.SetFloat(AnimationParameters.HitDirectionZ, hitDirection.y);
        }
        public AnimatorStateInfo GetCurrentAnimationState(int animationLayer) {
            return animator.GetCurrentAnimatorStateInfo(animationLayer);
        }
        public void ChangeAnimationState(int stateHashName, float transitionDuration, int animatorLayer) 
            => animator.CrossFade(stateHashName, transitionDuration, animatorLayer);
        public void EnableRootMotion(bool enable) => animator.applyRootMotion = enable;
        public float GetAnimatorFloat(int parameter) => animator.GetFloat(parameter);

        public bool IsInTransition(int layer) {
            return animator.IsInTransition(layer);
        }
    }
}