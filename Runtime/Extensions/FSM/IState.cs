using UnityEngine;

namespace Extensions.FSM {
    public interface IState { 
        public void OnEnter();
        public void Tick();
        public void FixedTick();
        public void OnExit();
        
        // Drawing gizmos for debugging, not necessary
        public Color GizmoState() => Color.clear;
    } 
}