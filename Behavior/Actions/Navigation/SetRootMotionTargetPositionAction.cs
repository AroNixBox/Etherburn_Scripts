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

    protected override Status OnStart() {
        var missingType = MissingType();
        if(missingType != null) {
            Debug.LogError($"{missingType} is missing.");
            return Status.Failure;
        }
        
        if(!_initialized) {
            _distanceDependentRootMotionDatas = RootMotionDataWrapper.Value
                .GetRootMotionData(RootMotionType.Value)
                .Where(data => !data.distanceIndependent)
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

            // Get the highest selectionProbability
            var highestProbabilityData = sortedList.First().Key;
            uint highestProbability = highestProbabilityData.selectionProbability;
            
            // Generate a random number between 1 and 100
            System.Random random = new System.Random();
            int randomNumber = random.Next(1, 101);
            
            // Check if the random number is less than or equal to the highest selectionProbability
            if (randomNumber <= highestProbability) {
                // success
                
                // Decrease the executionAmount by 1
                _distanceIndependentRootMotionDatas[highestProbabilityData] -= 1;
                if (_distanceIndependentRootMotionDatas[highestProbabilityData] == 0) {
                    // Remove the RootMotionData from the Dict if the executionAmount is 0
                    _distanceIndependentRootMotionDatas.Remove(highestProbabilityData);
                }
                
                return highestProbabilityData;
            }
        }

        RootMotionAnimationDataSO bestDistanceDependentRootMotionData = null;
        float bestDistance = float.MaxValue;
        var rmDataList = RootMotionDataWrapper.Value.GetRootMotionData(RootMotionType.Value);
        rmDataList.Shuffle();

        foreach (var rmData in rmDataList) {
            var rmWorldRootMotion = Self.Value.transform.TransformDirection(rmData.totalRootMotion);
            rmWorldRootMotion.y = 0;

            // Is the target position reachable?
            if (!NavMesh.SamplePosition(selfPosition + rmWorldRootMotion, out NavMeshHit hit, 0.1f, NavMesh.AllAreas)) {
                continue;
            }

            if (BasedOnTarget.Value) {
                var distanceToTarget = (Target.Value.transform.position - (selfPosition + rmWorldRootMotion)).magnitude;

                if (Mathf.Abs(distanceToTarget - MinAttackDistance) < bestDistance) {
                    bestDistance = Mathf.Abs(distanceToTarget - MinAttackDistance);
                    bestDistanceDependentRootMotionData = rmData;
                }
                continue;
            }
            
            bestDistanceDependentRootMotionData = rmData;
            break;
        }
        
        if(bestDistanceDependentRootMotionData == null) {
            // Fallback, check if we have an AttackClip with 0,0,0 RootMotion
            foreach (var rmData in rmDataList) {
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