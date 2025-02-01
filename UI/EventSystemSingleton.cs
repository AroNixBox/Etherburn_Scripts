using Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace UI {
    [RequireComponent(typeof(EventSystem), typeof( InputSystemUIInputModule))]
    public class EventSystemSingleton : Singleton<EventSystemSingleton> {
        public EventSystem EventSystem { get; private set; }
        public InputSystemUIInputModule InputSystemUIInputModule { get; private set; }
        protected override void Awake() {
            base.Awake();
        
            InputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
            EventSystem = GetComponent<EventSystem>();
        }
    }
}