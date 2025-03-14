using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(ScrollRect))]
    public class AutoScroll : MonoBehaviour {
        [SerializeField] float initialScrollDelay = 5f;
        [SerializeField] float scrollSpeed = 0.2f;
        [SerializeField] bool loopScrolling;
        [SerializeField] bool invertScrolling;

        ScrollRect _scrollRect;
        float _currentPosition;
        float _timer = 0f;

        void Awake() {
            _scrollRect = GetComponent<ScrollRect>();
            
            // Initialize position based on scroll direction
            _currentPosition = invertScrolling ? 1f : 0f;
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, _currentPosition);
            
            _scrollRect.enabled = false;
        }

        void Update() {
            // Wait for initial delay before scrolling
            _timer += Time.deltaTime;
            if (_timer < initialScrollDelay) {
                return;
            }
            
            // Calculate scroll direction
            float scrollDelta = scrollSpeed * Time.deltaTime;
            _currentPosition += invertScrolling ? -scrollDelta : scrollDelta;
            
            // Handle looping or stopping at the end
            if (invertScrolling) {
                if (_currentPosition < 0f) {
                    _currentPosition = loopScrolling ? 1f : 0f;
                }
            } else {
                if (_currentPosition > 1f) {
                    _currentPosition = loopScrolling ? 0f : 1f;
                }
            }

            // Apply scrolling
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, _currentPosition);
        }

        public void ResetScroll() {
            _currentPosition = invertScrolling ? 1f : 0f;
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, _currentPosition);
            _timer = 0f; // Reset timer to apply delay again
        }
    }
}