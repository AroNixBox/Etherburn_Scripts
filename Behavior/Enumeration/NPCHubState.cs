using Unity.Behavior;

// Defines "Idle States - Hub" for NPC
[BlackboardEnum]
public enum NPCHubState {
    None,
    Idle,
    Patrol,
    Sit
}