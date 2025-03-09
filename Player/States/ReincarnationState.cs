using Extensions.FSM;
using Player.Animation;
using ShaderControl;
using UnityEngine;

namespace Player.States {
    public class ReincarnationState : IState {
        readonly References _references;
        readonly AnimationController _animationController;
        readonly Mover _mover;
        readonly DissolveControl _dissolveControl;
        
        public ReincarnationState(References references) {
            _animationController = references.animationController;
            _mover = references.mover;
            _references = references;
            _dissolveControl = references.playerMeshDissolveControl;
        }
        public void OnEnter() {
            var saveManager = Game.Save.SaveManager.Instance;
            var playerPosition = saveManager.GetObjectPosition(_mover.gameObject.name);

            if (playerPosition == null) {
                Debug.LogError("No Teleport Data found, but still went into Teleport State");
                return;
            }
            if(_dissolveControl == null) {
                Debug.LogError("No Dissolve Control found, but still went into Reincarnation State");
                return;
            }
            
            _mover.Teleport((Vector3) playerPosition);
            _animationController.ChangeAnimationState(AnimationParameters.Reincarnation, 
                AnimationParameters.GetAnimationDuration(AnimationParameters.Reincarnation), 
                0);
            
            _ = _dissolveControl.ChangeDissolveMode(DissolveControl.DissolveMode.Materialize);
        }

        public void Tick() {
            
        }

        public void FixedTick() { }

        public void OnExit() {
            _references.ReincarnationEnded = false;
        }
    }
}