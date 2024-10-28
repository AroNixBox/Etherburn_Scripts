using System;
using UnityEngine;

namespace Sensor {
    public class TargetDetectionSensor : MonoBehaviour {
        [SerializeField] Transform modelHead;
        [SerializeField] Transform[] rayCheckOrigins;
        
        [SerializeField] uint maxTargets;
        [SerializeField] float detectionRadius = 5f;
        [SerializeField] float visionConeAngle = 45f;
        
        Extensions.VisionTargetQuery<Enemy.EnemyWarpTargetProvider> _visionEnemyWarpTargetQuery;
        
        void Start() {
            _visionEnemyWarpTargetQuery = new (modelHead, rayCheckOrigins, (int)maxTargets, detectionRadius, visionConeAngle);
        }
        
        public Transform GetNearestTargetInVisionCone() {
            return _visionEnemyWarpTargetQuery.GetNearestTargetInVisionCone()?.transform;
        }

        void OnDrawGizmos() {
            if (_visionEnemyWarpTargetQuery == null) { return; }
            
            _visionEnemyWarpTargetQuery.DrawVisionCone();
            _visionEnemyWarpTargetQuery.DrawDetectionRadius();
            _visionEnemyWarpTargetQuery.DrawLineToTarget();
        }
    }
}