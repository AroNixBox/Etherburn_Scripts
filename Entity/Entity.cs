using UnityEngine;

public class Entity : MonoBehaviour {
    [field: SerializeField] public EntityType EntityType { get; private set; }
}
public enum EntityType {
    Player,
    Enemy,
    NPC
}
