namespace Extensions.FSM {
    public interface ISubStateMachine : IState {
        
        public string GetCurrentStateName();
    }
}
