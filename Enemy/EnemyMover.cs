using UnityEngine;
using UnityEngine.AI;

namespace Enemy {
    public class EnemyMover : MonoBehaviour {
        [SerializeField] NavMeshAgent agent;
        [SerializeField] Animator animator;
        [SerializeField] float velocitySmoothing = 0.1f;
    
        static readonly int SpeedMagnitude = Animator.StringToHash("SpeedMagnitude");
        const float ANIMATION_DAMPING = 0.1f;
    
        Vector3 _smoothDeltaPosition;
        Vector3 _velocity;
    
        void Awake() {
            agent.updatePosition = false;
            // TODO: If wanna control rotation, set this to true
            agent.updateRotation = true;
        }
    
        void Update() {
            HandleInput();
            UpdateMovement();
        }
    
        void UpdateMovement() {
            if (!agent.hasPath) {
                _smoothDeltaPosition = Vector3.zero;
                animator.SetFloat(SpeedMagnitude, 0, 0.1f, Time.deltaTime);
                
                return;
            }
            

            // Smoothing movement
            Vector3 worldDeltaPosition = agent.desiredVelocity;
            _smoothDeltaPosition = Vector3.SmoothDamp(
                _smoothDeltaPosition,
                worldDeltaPosition,
                ref _velocity,
                velocitySmoothing
            );
        
            var speed = _smoothDeltaPosition.magnitude;
            animator.SetFloat(SpeedMagnitude, speed, ANIMATION_DAMPING, Time.deltaTime);
        }
    
        void HandleInput() {
            if (!Input.GetMouseButtonDown(0)) { return; }
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) { return; }
            
            agent.SetDestination(hit.point);
                
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("GroundLocomotion")) { return; }
            animator.CrossFadeInFixedTime("GroundLocomotion", 0.1f);
        }
    
        public void AnimatorMove(Vector3 rootPosition) {
            transform.position = rootPosition;
            agent.nextPosition = rootPosition;
        }
    }
}