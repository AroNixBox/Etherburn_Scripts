using System;
using Unity.Behavior;

[BlackboardEnum]
public enum NPCAnimationStates {
	None,
    GroundLocomotion,
    Hub_Idle,
    Hub_Sit,
	AttackA,
	AttackB,
	Eat,
	HurtA,
	HurtB,
	Die,
	WaitForExecution
}
