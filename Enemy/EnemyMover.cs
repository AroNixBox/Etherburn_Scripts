using UnityEngine;
using UnityEngine.AI;

namespace Enemy {
    public class EnemyMover : MonoBehaviour {
        [SerializeField] NavMeshAgent agent;
        void Awake() {
            agent.updatePosition = false;
            // TODO: If wanna control rotation, set this to true
            agent.updateRotation = false;
        }
        public void AnimatorMove(Vector3 rootPosition) {
            var newPosition = rootPosition;
            newPosition.y = agent.nextPosition.y;
            transform.position = newPosition;
            agent.nextPosition = newPosition;
        }
    }
}