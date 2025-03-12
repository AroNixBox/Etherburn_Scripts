using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Menu {
    public class SelectionVisualizer : MonoBehaviour {
        [SerializeField] List<InteractableElement> interactableElements;

        void Start() {
            SetupInteractableElements();
        }

        void SetupInteractableElements() {
            foreach (var element in interactableElements) {
                AddEventTriggers(element.interactable, element.selectImage);
            }
        }

        void AddEventTriggers(GameObject interactable, GameObject selectImage) {
            Debug.Log($"Adding Event Triggers to {interactable.name}");

            EventTrigger eventTrigger = interactable.GetComponent<EventTrigger>();
            if (eventTrigger == null) {
                eventTrigger = interactable.AddComponent<EventTrigger>();
            }

            // Create and add Select entry
            EventTrigger.Entry selectEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener(_ => { selectImage.SetActive(true); });
            eventTrigger.triggers.Add(selectEntry);

            // Create and add Deselect entry
            EventTrigger.Entry deselectEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.Deselect
            };
            deselectEntry.callback.AddListener(_ => { selectImage.SetActive(false); });
            eventTrigger.triggers.Add(deselectEntry);

            // Create and add PointerEnter entry
            EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener(_ => { selectImage.SetActive(true); });
            eventTrigger.triggers.Add(pointerEnterEntry);

            // Create and add PointerExit entry
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener(_ => { selectImage.SetActive(false); });
            eventTrigger.triggers.Add(pointerExitEntry);

            // Create and add Submit entry (for controller)
            EventTrigger.Entry submitEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.Submit
            };
            submitEntry.callback.AddListener(_ => {
                // Handle the submit action here
                Debug.Log("Interactable element submitted");
            });
            eventTrigger.triggers.Add(submitEntry);
            
            // Disable all select images initially
            selectImage.SetActive(false);
        }

        
        [System.Serializable]
        public class InteractableElement {
            public GameObject interactable;
            public GameObject selectImage;
        }
    }
}
