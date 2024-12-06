using UnityEngine;

namespace Game {
    public class CursorManager : MonoBehaviour {
        [SerializeField] bool hideCursor;
        public static CursorManager Instance { get; private set; }
        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }
        void Start() {
            SetCursorVisible(!hideCursor);
            SetCursorLockMode(hideCursor ? CursorLockMode.Locked : CursorLockMode.None);
        }
        public void SetCursorVisible(bool visible) {
            Cursor.visible = visible;
        }
        public void SetCursorLockMode(CursorLockMode lockMode) {
            Cursor.lockState = lockMode;
        }
        public CursorLockMode GetCursorLockMode() {
            return hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }
        public bool GetCursorVisible() {
            return !hideCursor;
        }
    }
}
