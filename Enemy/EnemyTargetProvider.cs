using Behavior.Events;
using UnityEngine;

namespace Enemy {
    public class EnemyTargetProvider : MonoBehaviour, IRequireNPCStateChannel {
        [SerializeField] GameObject warpTarget;
        [SerializeField] float distanceToWarpTarget = 1f;
        NpcStateChanged _npcStateChannel;
        Target _warpTarget;
        
        // Called on one Enemy is in vision cone of Player and is closest to the player.
        // We position the target between the player and this (Enemy) with [distanceToWarpTarget] distance
        // and look at the this component (Enemy)
        public Target ProvideWarpTarget(Transform playerTransform, bool isQuery = true) {
            // If we are querying, we dont know if we are going to warp or not
            if (isQuery) {
                _warpTarget ??= CreateWarpTarget(playerTransform);
            
                // Only Rotate around Target & Look at it when the Target is requested. 
                _warpTarget.RotateAroundParent();
                _warpTarget.LookAtParent();
            }
            else { // If we are not querying, we are going to warp, so we are taking the last target that was calculated.
                _npcStateChannel?.SendEventMessage(NPCState.WaitForExecution);
            }
            
            return _warpTarget;
        }

        Target CreateWarpTarget(Transform playerTransform) {
            var newTarget = Instantiate(warpTarget);
            var newWarpTarget = new Target(newTarget.transform, playerTransform, transform, distanceToWarpTarget);
            return newWarpTarget;
        }

        public void AssignEventChannel(NpcStateChanged npcStateChannel) {
            _npcStateChannel = npcStateChannel;
        }
    }
}