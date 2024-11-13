using System.Linq;
using UnityEngine;
using Extensions;
using Player.Cam;
using Sirenix.OdinInspector;

namespace Player.Ability
{
    public class AbilityTargetQuery : MonoBehaviour {
        [Header("References")]
        [SerializeField] References references;
        [SerializeField, Required] Transform headHeight;
        
        OrbitalController _orbitalController;
        Transform[] _rayCheckOrigins;
        int _maxTargets;
        
        VisionTargetQuery<Entity> _visionEnemyWarpTargetQuery;

        void Awake() {
            _orbitalController = references.orbitalController;
        }

        void Start() {
            _maxTargets = references.maxTargetToCheckAround;
            _rayCheckOrigins = references.rayCheckOrigins;
            var detectionRadius = references.detectionRadius;
            var visionConeAngle = references.visionConeAngle;
            
            _visionEnemyWarpTargetQuery = new VisionTargetQuery<Entity>(headHeight, _rayCheckOrigins, _maxTargets, detectionRadius, visionConeAngle);
        }

        public Entity GetWarpTargetProvider(EntityType entityType) {
            var lockedOnTarget = _orbitalController.LockedOnEnemyTarget;
            
            if(lockedOnTarget != null) {
                // Return Early, because we have a locked on target
                return lockedOnTarget;
            }
            
            var entities = _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted();
            if(entities.Count == 0) { return null; }

            return entities.FirstOrDefault(entity => entity.EntityType == entityType);
        }
        
        void OnDrawGizmos() {
            if(_visionEnemyWarpTargetQuery == null) { return; }
            
            _visionEnemyWarpTargetQuery.DrawDetectionRadius();
            _visionEnemyWarpTargetQuery.DrawVisionCone();
            
            var cameraTarget = _orbitalController.LockedOnEnemyTarget != null ? _orbitalController.LockedOnEnemyTarget.transform : null;
            _visionEnemyWarpTargetQuery.DrawLineToTarget(cameraTarget);
        }
    }
}