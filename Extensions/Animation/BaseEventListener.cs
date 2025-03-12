using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Animation {
    public class BaseEventListener : MonoBehaviour {
        [SerializeField] List<AnimationEventAction> specialAnimEvents = new();
        void SpecialAnimationAction(string eventName) {
            var matchedEvent = specialAnimEvents.Find(e => e.eventName == eventName);

            matchedEvent?.action?.Invoke();
        }
    }
}