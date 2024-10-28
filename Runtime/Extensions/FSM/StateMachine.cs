using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions.FSM {
    public class StateMachine {
        public event Action<string> OnDebugStateChanged = delegate { };
        IState _currentState;
        
        readonly Dictionary<IState, List<Transition>> _transitions = new ();
        List<Transition> _currentTransitions = new ();
        readonly List<Transition> _anyTransitions = new ();
        
        static readonly List<Transition> EmptyTransitions = new (0);
        /// <summary>
        /// Runs the current state's Tick method and tracks transitions
        /// </summary>
        public void Tick() {
            var transition = GetTransition();
            
            if (transition != null) {
                SetState(transition.To);
            }
            
            _currentState?.Tick();
        }
        
        public void FixedTick() {
            _currentState?.FixedTick();
        }

        /// <summary>
        /// Start the StateMachine
        /// </summary>
        /// <param name="state"> Initial State</param>
        public void SetInitialState(IState state) => SetState(state);
        
        void SetState(IState state) {
            _currentState?.OnExit();
            _currentState = state;
            
            _transitions.TryGetValue(_currentState, out _currentTransitions);
            _currentTransitions ??= EmptyTransitions;
            
            // Debug    
            OnDebugStateChanged.Invoke(GetCurrentStateName());
            
            _currentState.OnEnter();
        }
    
        public void AddTransition(IState from, IState to, Func<bool> predicate) {
            // Check if there are already transitions from the 'from' state
            if (!_transitions.TryGetValue(from, out var transitions)) {
                // No transitions from the 'from' state, create a new list
                transitions = new List<Transition>();
                // Add to dict
                _transitions[from] = transitions;
            }
            
            // Add the transition we passed in to the List of transitions of the 'from' state
            transitions.Add(new Transition(from, to, predicate));
        }
    
        public void AddAnyTransition(IState state, Func<bool> predicate) {
            // Add the transition to the list of any transitions
            _anyTransitions.Add(new Transition(null, state, predicate));
        }
    
        Transition GetTransition() {
            // Iterate through all any state transitions first
            foreach (var transition in _anyTransitions.Where(transition => transition.Condition() 
                                                                           // Dont Allow Any to Same State Transition.
                                                                           && transition.To != _currentState)) {
                // If a condition is met, return that transition
                return transition;
            }

            // If no 'any' transition is found, check the current state's transitions
            return _currentTransitions.FirstOrDefault(transition => transition.Condition());
        }
        
        class Transition {
            public IState From { get; }
            public Func<bool> Condition { get; }
            public IState To { get; }
    
            public Transition(IState from, IState to, Func<bool> condition) {
                From = from;
                To = to;
                Condition = condition;
            }
        }
        
        public IState GetCurrentState() {
            return _currentState;
        }
        public Color GetGizmoColor() {
            return _currentState?.GizmoState() ?? Color.black;
        }
        string GetCurrentStateName() {
            return _currentState?.GetType().Name ?? "No State";
        }
    }
}
