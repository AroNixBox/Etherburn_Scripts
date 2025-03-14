using UnityEngine;

namespace Game {
    public class GameWinTrigger : MonoBehaviour {
        public void TriggerGameWin() {
            var gameBrain = GameBrain.Instance;
            if(gameBrain == null) {
                return;
            }

            _ = gameBrain.UninitializeGame(false, true);
        }
    }
}
