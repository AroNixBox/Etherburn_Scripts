using UnityEngine;

namespace Attributes {
    [System.Serializable]
    public class AttributeData {
        [Range(0, 200)] public float damage;
        [Range(0, 100)] public float stamina;
        [Range(0, 100)] public float ultimate;
    }
}