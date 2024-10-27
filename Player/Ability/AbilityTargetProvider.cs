using System;
using UnityEngine;
using Enemy;
using Extensions;
using Player.Cam;
using Sirenix.OdinInspector;

namespace Player.Ability
{
    public class AbilityTargetProvider : MonoBehaviour {
        [Header("References")]
        [SerializeField] References references;
        [SerializeField, Required] Transform headHeight;
        
        OrbitalController _orbitalController;
        Transform[] _rayCheckOrigins;
        int _maxTargets;
        
        VisionTargetQuery<EnemyWarpTargetProvider> _visionEnemyWarpTargetQuery;

        void Awake() {
            _orbitalController = references.orbitalController;
        }

        void Start() {
            _maxTargets = references.maxTargetToCheckAround;
            _rayCheckOrigins = references.rayCheckOrigins;
            var detectionRadius = references.detectionRadius;
            var visionConeAngle = references.visionConeAngle;
            
            _visionEnemyWarpTargetQuery = new VisionTargetQuery<EnemyWarpTargetProvider>(headHeight, _rayCheckOrigins, _maxTargets, detectionRadius, visionConeAngle);
        }

        public EnemyWarpTargetProvider GetWarpTargetProvider() {
            var lockedOnTarget = _orbitalController.LockedOnTarget;
            
            if(lockedOnTarget != null) {
                // Return Early, because we have a locked on target
                return lockedOnTarget;
            }
            
            return _visionEnemyWarpTargetQuery.GetNearestTargetInVisionCone();
        }
        
        void OnDrawGizmos() {
            if(_visionEnemyWarpTargetQuery == null) { return; }
            
            _visionEnemyWarpTargetQuery.DrawDetectionRadius();
            _visionEnemyWarpTargetQuery.DrawVisionCone();
            
            var cameraTarget = _orbitalController.LockedOnTarget != null ? _orbitalController.LockedOnTarget.transform : null;
            _visionEnemyWarpTargetQuery.DrawLineToTarget(cameraTarget);
        }
    }
}