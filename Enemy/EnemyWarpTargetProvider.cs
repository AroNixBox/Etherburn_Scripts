using UnityEngine;

namespace Enemy {
    public class EnemyWarpTargetProvider : MonoBehaviour {
        [SerializeField] GameObject warpTarget;
        [SerializeField] float distanceToWarpTarget = 1f;
        Target _warpTarget;
        
        // Called on one Enemy is in vision cone of Player and is closest to the player.
        // We position the target between the player and this (Enemy) with [distanceToWarpTarget] distance
        // and look at the this component (Enemy)
        public Target ProvideWarpTarget(Transform playerTransform) {
            _warpTarget ??= CreateWarpTarget(playerTransform);
            
            // Only Rotate around Target & Look at it when the Target is requested. 
            _warpTarget.RotateAroundParent();
            _warpTarget.LookAtParent();
            
            return _warpTarget;
        }

        Target CreateWarpTarget(Transform playerTransform) {
            var newTarget = Instantiate(warpTarget);
            var newWarpTarget = new Target(newTarget.transform, playerTransform, transform, distanceToWarpTarget);
            return newWarpTarget;
        }
    }
}