using System.Linq;
using Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu {
    [RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
    public class ScrollAreaController : MonoBehaviour {
        ScrollRect _scrollRect;
        RectTransform _scrollRectTransform;
        ButtonElement[] _buttonElements;
        float _lastButtonPosY;

        void Awake() {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRectTransform = GetComponent<RectTransform>();
        }

        void Start() {
            var buttons = _scrollRect.content.GetComponentsInChildren<Button>();
            _buttonElements = new ButtonElement[buttons.Length];

            for (var i = 0; i < buttons.Length; i++) {
                var index = i;
                _buttonElements[index] = new ButtonElement {
                    Button = buttons[index],
                    RectTransform = buttons[index].GetComponent<RectTransform>()
                };

                var trigger = buttons[index].gameObject.AddComponent<EventTrigger>();
                
                AddEventTrigger(trigger, EventTriggerType.Select, _ => {
                    ScrollToButton(buttons[index]);
                });

                // PointerEnter-Ereignis für Maus-Hover hinzufügen
                AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => {
                    ScrollToButton(buttons[index]);
                });
            }
        }

        /// <summary>
        /// Adds an event trigger to the given trigger with the given event type and action.
        /// </summary>
        void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action) {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Scrolls the scroll area to the given button.
        /// </summary>
        void ScrollToButton(Button button) {
            if (!InputUtils.WasLastInputController()) return;

            // Find the button element
            var buttonElement = _buttonElements.FirstOrDefault(be => be.Button == button);
            if (buttonElement == null) return;

            // Local position of the button
            var buttonRectTransform = buttonElement.RectTransform;
            var contentRectTransform = _scrollRect.content;
            var buttonLocalPos = contentRectTransform.InverseTransformPoint(buttonRectTransform.position);
            var buttonPosY = buttonLocalPos.y;

            // Only scroll if the button is not already centered
            if (!(Mathf.Abs(buttonPosY - _lastButtonPosY) > 10)) { return; }
            
            var scrollHeight = _scrollRectTransform.rect.height;

            // Calculate the target position of the button
            var targetPosY = buttonPosY + (scrollHeight / 2) - (buttonRectTransform.rect.height / 2);

            // Scroll to the target position
            var contentHeight = _scrollRect.content.rect.height;
            var normalizedPosY = Mathf.Clamp01(targetPosY / (contentHeight - scrollHeight));

            // Scroll to the target position
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, normalizedPosY);
            
            _lastButtonPosY = buttonPosY;
        }

        class ButtonElement {
            public Button Button;
            public RectTransform RectTransform;
        }
    }
}
