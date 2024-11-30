using Unity.Behavior;

[BlackboardEnum]
public enum NPCState {
	None,
    Patrol,
	Agressive,
	Hurt,
	Die,
	WaitForExecution
}
