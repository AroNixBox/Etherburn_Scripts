using Extensions.FSM;
using UnityEngine;

namespace Player.States {
    public class TeleportState : IState {
        readonly Animation.AnimationController _animationController;
        readonly Mover _mover;
        
        public TeleportState(References references) {
            _animationController = references.animationController;
            _mover = references.mover;
        }

        public bool TeleportEnded { get; private set; }

        public void OnEnter() {
            var saveManager = Game.Save.SaveManager.Instance;
            var playerPosition = saveManager.GetObjectPosition(_mover.gameObject.name);

            if (playerPosition == null) {
                Debug.LogError("No Teleport Data found, but still went into Teleport State");
                return;
            }
            
            _mover.Teleport((Vector3) playerPosition);
            TeleportEnded = true;
        }

        public void Tick() {
            
        }

        public void FixedTick() { }

        public void OnExit() {
            
        }
    }
}