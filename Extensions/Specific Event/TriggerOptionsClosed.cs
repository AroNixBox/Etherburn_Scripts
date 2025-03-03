using Game;
using UnityEngine;

namespace Extensions.Specific_Event {
    public class TriggerOptionsClosed : MonoBehaviour {
        public void OptionsClosed() {
            var gameBrain = GameBrain.Instance;
            if (gameBrain == null) {
                Debug.LogError("GameBrain is not in the scene", transform);
                return;
            }
            GameBrain.Instance.PauseToggleTriggered = true;
        }
    }
}
