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
        ButtonElement[] _buttonPairs;
        float _lastButtonPosY;

        void Awake() {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRectTransform = GetComponent<RectTransform>();
        }

        void Start() {
            var buttons = _scrollRect.content.GetComponentsInChildren<Button>();
            _buttonPairs = new ButtonElement[buttons.Length];

            for (var i = 0; i < buttons.Length; i++) {
                var index = i; // Create a local copy of the loop variable
                _buttonPairs[index] = new ButtonElement {
                    Button = buttons[index],
                    RectTransform = buttons[index].GetComponent<RectTransform>(),
                    BackgroundImage = buttons[index].transform.parent.GetComponentInChildren<Image>()
                };

                var trigger = buttons[index].gameObject.AddComponent<EventTrigger>();

                // Add Select event for controller
                AddEventTrigger(trigger, EventTriggerType.Select, _ => {
                    ScrollToButton(buttons[index]);
                    EnableUISelectionRow(_buttonPairs[index].BackgroundImage);
                });

                // Add PointerEnter event for mouse hover
                AddEventTrigger(trigger, EventTriggerType.PointerEnter, _ => {
                    ScrollToButton(buttons[index]);
                    EnableUISelectionRow(_buttonPairs[index].BackgroundImage);
                });

                // Add Deselect event for controller
                AddEventTrigger(trigger, EventTriggerType.Deselect, _ => DisableAllBackgroundImages());
            }

            var eventSystem = EventSystem.current;
            if (eventSystem == null) {
                Debug.LogError("No EventSystem found in scene");
                return;
            }

            var selectedGameObject = eventSystem.currentSelectedGameObject;
            
            foreach (var buttonPair in _buttonPairs) {
                buttonPair.BackgroundImage.enabled = buttonPair.Button.gameObject == selectedGameObject;
            }
        }

        /// <summary>
        /// Adds an event trigger entry to the specified EventTrigger.
        /// </summary>
        void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action) {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Enables the background image of the selected button.
        /// </summary>
        void EnableUISelectionRow(Image image) {
            foreach (var buttonPair in _buttonPairs) {
                buttonPair.BackgroundImage.enabled = buttonPair.BackgroundImage == image;
            }
        }

        /// <summary>
        /// Disables all background images.
        /// </summary>
        void DisableAllBackgroundImages() {
            foreach (var buttonPair in _buttonPairs) {
                buttonPair.BackgroundImage.enabled = false;
            }
        }

        /// <summary>
        /// Scrolls the scroll rect to center the specified button.
        /// </summary>
        void ScrollToButton(Button button) {
            if (!InputUtils.IsUsingController()) return;

            // Find the ButtonElement for the given button
            var buttonElement = _buttonPairs.FirstOrDefault(bp => bp.Button == button);
            if (buttonElement == null) return;

            // Calculate the local offset of the button relative to the content
            var buttonRectTransform = buttonElement.RectTransform;
            var contentRectTransform = _scrollRect.content;
            var buttonLocalPos = contentRectTransform.InverseTransformPoint(buttonRectTransform.position);
            var buttonPosY = buttonLocalPos.y;

            // Scroll only if the difference in Y value is significant
            if (!(Mathf.Abs(buttonPosY - _lastButtonPosY) > 10)) { return; }
            
            var scrollHeight = _scrollRectTransform.rect.height;

            // Calculate how much we need to scroll to center the button
            var targetPosY = buttonPosY + (scrollHeight / 2) - (buttonRectTransform.rect.height / 2);

            // Calculate the scrollable range and the normalized position
            var contentHeight = _scrollRect.content.rect.height;
            var normalizedPosY = Mathf.Clamp01(targetPosY / (contentHeight - scrollHeight));

            // Set the scroll position only vertically
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, normalizedPosY);

            // Save the current Y position for the next selection
            _lastButtonPosY = buttonPosY;
        }

        class ButtonElement {
            public Button Button;
            public RectTransform RectTransform;
            public Image BackgroundImage;
        }
    }
}