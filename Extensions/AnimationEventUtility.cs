using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Extensions {
#if UNITY_EDITOR
    public static class AnimationEventUtility {
        public static void AddOrReplaceAnimationEvent(AnimationClip clip, float time, string functionName) {
            // Get existing events
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);

            // Create a new list for the events
            var newEventsList = new List<AnimationEvent>();

            // Filter out events with the same function name
            foreach (var evt in events) {
                if (evt.functionName != functionName) {
                    newEventsList.Add(evt);
                }
            }

            // Create a new event
            AnimationEvent newEvent = new AnimationEvent {
                time = time,
                functionName = functionName
            };

            // Add the new event to the list
            newEventsList.Add(newEvent);

            // Set the new events array to the clip
            AnimationUtility.SetAnimationEvents(clip, newEventsList.ToArray());
        }
    } 
#endif
}