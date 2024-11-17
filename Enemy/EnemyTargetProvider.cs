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
        public Target ProvideWarpTarget(Transform playerTransform) {
            _warpTarget ??= CreateWarpTarget(playerTransform);
            
            // Only Rotate around Target & Look at it when the Target is requested. 
            _warpTarget.RotateAroundParent();
            _warpTarget.LookAtParent();
            
            _npcStateChannel?.SendEventMessage(NPCState.WaitForExecution);
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