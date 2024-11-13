using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Sensor {
    public class EntityDetectionSensor : MonoBehaviour {
        [SerializeField] Transform modelHead;
        [SerializeField] Transform[] rayCheckOrigins;
        
        [SerializeField] uint maxTargets;
        [SerializeField] float detectionRadius = 5f;
        [SerializeField] float visionConeAngle = 45f;
        
        VisionTargetQuery<Entity> _visionEnemyWarpTargetQuery;
        
        void Start() {
            _visionEnemyWarpTargetQuery = new VisionTargetQuery<Entity>(modelHead, rayCheckOrigins, (int)maxTargets, detectionRadius, visionConeAngle);
        }
        public List<Entity> GetAllTargetsInVisionConeSorted() {
            return _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted();
        }

        void OnDrawGizmos() {
            if (_visionEnemyWarpTargetQuery == null) { return; }
            
            _visionEnemyWarpTargetQuery.DrawVisionCone();
            _visionEnemyWarpTargetQuery.DrawDetectionRadius();
            _visionEnemyWarpTargetQuery.DrawLineToTarget();
        }
    }
}