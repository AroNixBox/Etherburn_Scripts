using UnityEngine;

public class HideCursor : MonoBehaviour {
    [SerializeField] bool hideCursor;
    
    void Start() {
        Cursor.visible = !hideCursor;
        Cursor.lockState = hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
