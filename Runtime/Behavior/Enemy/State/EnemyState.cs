using System;
using Unity.Behavior;

[BlackboardEnum]
public enum EnemyState {
    Patrol,
	Chase,
	Attack,
	Eat,
	Hurt,
	Die
}
