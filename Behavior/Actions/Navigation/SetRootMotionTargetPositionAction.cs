using System;
using System.Collections.Generic;
using System.Linq;
using Behavior.Enemy.State.Animation;
using Motion.RootMotion;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

/* Set Root Motion Target Position
 * 
 * 1. Picks an Animation Clip with a fitting RootMotion- Distance which is closest to the target + MinAttackDistance
 * 2. Replaces the current Animation Clip with the best fitting Root Motion Data
 * 3. Sets the RootMotionEndPosition to the position where the Animation ends
 */
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Root Motion Target Position", story: "Set [RootMotionEndPosition] position from [RootMotionDataWrapper] in [CurrentAnimationState] based on [Self] position",
    category: "Action/Navigation/RootMotion", id: "2824f299514320b8fb4d4556125a2faf")]
public partial class SetRootMotionTargetPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> RootMotionEndPosition;
    [SerializeReference] public BlackboardVariable<RootMotionDataWrapper> RootMotionDataWrapper;
    [SerializeReference] public BlackboardVariable<AnimationController> AnimationController;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> BasedOnTarget;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<NPCAnimationStates> CurrentAnimationState;
    [SerializeReference] public BlackboardVariable<float> MinAttackDistance;
    [SerializeReference] public BlackboardVariable<RootMotionDataWrapper.RootMotionType> RootMotionType;
    
    bool _initialized;
    Dictionary<RootMotionAnimationDataSO, int> _distanceIndependentRootMotionDatas = new();
    List<RootMotionAnimationDataSO> _distanceDependentRootMotionDatas = new();
    List<RootMotionAnimationDataSO> _distanceDependentRootMotionDatasWithImpactRadius = new();
    RootMotionAnimationDataSO _lastUsedAttack;

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        if(!_initialized) {
            _distanceDependentRootMotionDatas = RootMotionDataWrapper.Value
                .GetRootMotionData(RootMotionType.Value)
                .Where(data => !data.distanceIndependent && !data.hasAttackRadius)
                .ToList();
            
            _distanceDependentRootMotionDatasWithImpactRadius = RootMotionDataWrapper.Value
                .GetRootMotionData(RootMotionType.Value)
                .Where(data => !data.distanceIndependent && data.hasAttackRadius)
                .ToList();
            
            _distanceIndependentRootMotionDatas = RootMotionDataWrapper.Value
                .GetRootMotionData(RootMotionType.Value)
                .Where(data => data.distanceIndependent)
                .ToDictionary(data => data, data => (int)data.executionAmount);
            
            _initialized = true;
        }

        Vector3 selfPosition = Self.Value.transform.position;
        RootMotionAnimationDataSO bestRootMotionData = FindBestRootMotionData(selfPosition);

        if (bestRootMotionData == null) {
            Debug.LogError("No fitting Root Motion Data found.");
            return Status.Failure;
        }
        
        // Which position is the Animation ending
        RootMotionEndPosition.Value = selfPosition + Self.Value.transform.TransformDirection(bestRootMotionData.totalRootMotion);
        
        // Replace the current Animation Clip with the best fitting Root Motion Data
        var oldAnimationClip = AnimationController.Value.GetInitialAttackClip(CurrentAnimationState.Value);
        AnimationController.Value.ReplaceClipFromOverrideController(oldAnimationClip, bestRootMotionData.clip);
        
        return Status.Success;
    }

    Type MissingType() {
        if(ReferenceEquals(RootMotionDataWrapper.Value, null)) { return typeof(RootMotionDataWrapper); }
        if(ReferenceEquals(AnimationController.Value, null)) { return typeof(AnimationController); }
        if(ReferenceEquals(Self.Value, null)) { return typeof(GameObject); }
        
        return null; // If all checks passed, no type is missing
    }

    RootMotionAnimationDataSO FindBestRootMotionData(Vector3 selfPosition) {
        if (_distanceIndependentRootMotionDatas.Count > 0) {
            // Sort the list by selectionProbability in descending order
            var sortedList = _distanceIndependentRootMotionDatas
                .OrderByDescending(kvp => kvp.Key.selectionProbability)
                .ToList();
            
            RootMotionAnimationDataSO highestReachableProbabilityData = null;
            
            //Get the highest selectionProbability that can be reached from the NavMeshAgent
            foreach (var rmData in sortedList) {
                var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.Key.totalRootMotion);
                rmWorldRootMotion.y = 0;
                
                // Check if we can reach the target position
                if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
                    continue;
                }
                
                // If the target position is reachable, set the highestReachableProbabilityData and break the loop
                highestReachableProbabilityData = rmData.Key;
                break;
            }
            
            // If the highestReachableProbabilityData is not null, means we found at least one reachable RootMotionData without distance dependency
            if (highestReachableProbabilityData != null) {
                uint highestProbability = highestReachableProbabilityData.selectionProbability;
            
                // Generate a random number between 1 and 100
                System.Random random = new System.Random();
                int randomNumber = random.Next(1, 101);
            
                // Check if the random number is less than or equal to the highest selectionProbability
                if (randomNumber <= highestProbability) {
                    // success
                
                    // Decrease the executionAmount by 1
                    _distanceIndependentRootMotionDatas[highestReachableProbabilityData] -= 1;
                    if (_distanceIndependentRootMotionDatas[highestReachableProbabilityData] == 0) {
                        // Remove the RootMotionData from the Dict if the executionAmount is 0
                        _distanceIndependentRootMotionDatas.Remove(highestReachableProbabilityData);
                    }
                
                    // Bail out early, since we have a RootMotionData without distance dependency that won the probability check
                    return highestReachableProbabilityData;
                }
            }
        }

        RootMotionAnimationDataSO bestDistanceDependentRootMotionData = null;
        float bestDistance = float.MaxValue;
        _distanceDependentRootMotionDatas.Shuffle();

        foreach (var rmData in _distanceDependentRootMotionDatas) {
            var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
            rmWorldRootMotion.y = 0;

            // Is the target position reachable?
            if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
                continue;
            }

            if (BasedOnTarget.Value) {
                var distanceToTarget = (Target.Value.transform.position - (selfPosition + rmWorldRootMotion)).magnitude;

                if (Mathf.Abs(distanceToTarget - MinAttackDistance) < bestDistance && rmData != _lastUsedAttack) {
                    bestDistance = Mathf.Abs(distanceToTarget - MinAttackDistance);
                    bestDistanceDependentRootMotionData = rmData;
                }
                continue;
            }

            if (rmData != _lastUsedAttack) {
                bestDistanceDependentRootMotionData = rmData;
                break;
            }
        }

        foreach (var rmData in _distanceDependentRootMotionDatasWithImpactRadius) {
            var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
            rmWorldRootMotion.y = 0;

            if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
                continue;
            }

            if (BasedOnTarget.Value) {
                var distanceToTarget = (Target.Value.transform.position - (selfPosition + rmWorldRootMotion)).magnitude;

                if (distanceToTarget <= rmData.attackRadius) {
                    float distancePercentage = distanceToTarget / rmData.attackRadius;
                    float accuracyMultiplier = 1.0f - distancePercentage; // Reduce accuracy based on distance percentage

                    float adjustedDistance = Mathf.Abs(distanceToTarget - MinAttackDistance) * accuracyMultiplier;

                    if (adjustedDistance < bestDistance && rmData != _lastUsedAttack) {
                        bestDistance = adjustedDistance;
                        bestDistanceDependentRootMotionData = rmData;
                    }
                }
            }
        }

        // If no alternative found, allow the last used attack
        if (bestDistanceDependentRootMotionData == null) {
            bestDistanceDependentRootMotionData = _lastUsedAttack;
        }

        // Update the last used attack
        _lastUsedAttack = bestDistanceDependentRootMotionData;

        if(bestDistanceDependentRootMotionData == null) {
            // Fallback, check if we have an AttackClip with 0,0,0 RootMotion
            foreach (var rmData in _distanceDependentRootMotionDatas) {
                if (rmData.totalRootMotion == Vector3.zero) {
                    bestDistanceDependentRootMotionData = rmData;
                    break;
                }
            }
        }

        return bestDistanceDependentRootMotionData;
    }

    protected override Status OnUpdate() {
        return Status.Success;
    }

    protected override void OnEnd() { }
}