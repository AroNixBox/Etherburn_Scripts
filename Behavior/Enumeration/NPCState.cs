using System;
using Unity.Behavior;

[BlackboardEnum]
public enum NPCState {
	None,
    Patrol,
	Combat,
	Hurt,
	Die
}
