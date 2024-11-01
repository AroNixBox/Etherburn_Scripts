using UnityEngine;
using UnityEngine.AI;

namespace Enemy {
    public class EnemyMover : MonoBehaviour {
        [SerializeField] NavMeshAgent agent;
        void Awake() {
            agent.updatePosition = false;
            // TODO: If wanna control rotation, set this to true
            agent.updateRotation = true;
        }
        public void AnimatorMove(Vector3 rootPosition) {
            transform.position = rootPosition;
            agent.nextPosition = rootPosition;
        }
    }
}