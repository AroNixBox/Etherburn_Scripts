using System;
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
        [SerializeField] bool debug;
        
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
            
            _visionEnemyWarpTargetQuery = new VisionTargetQuery<Entity>.Builder()
                .SetHead(headHeight)
                .SetRayCheckOrigins(_rayCheckOrigins)
                .SetMaxTargets(_maxTargets)
                .SetDetectionRadius(detectionRadius)
                .SetVisionConeAngle(visionConeAngle)
                .SetDebug(debug)
                .Build<Entity>();
        }

        public Entity GetWarpTargetProvider(EntityType entityType) {
            var lockedOnTarget = _orbitalController.LockedOnEnemyTarget;
            
            if(lockedOnTarget != null) {
                // Return Early, because we have a locked on target
                return lockedOnTarget;
            }
            
            var entityManager = EntityManager.Instance;
            if (entityManager == null) {
                Debug.LogError("Entity Manager ist not in the Scene!", transform);
                return null;
            }
            
            var allEnemies = entityManager.GetEntitiesOfType(EntityType.Enemy, out _); // TODO: Use "_" if want to perform something when no more Enemies are alive
            var allEntitiesInVisionCone = _visionEnemyWarpTargetQuery.GetAllTargetsInVisionConeSorted(allEnemies);
            if(allEntitiesInVisionCone.Count == 0) { return null; }
            return allEntitiesInVisionCone.FirstOrDefault(entity => entity.EntityType == entityType);
        }

        void OnDestroy() {
            _visionEnemyWarpTargetQuery.Dispose();
        }
    }
}