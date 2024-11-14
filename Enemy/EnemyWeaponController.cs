using System;
using UnityEngine;

namespace Enemy {
    public class EnemyWeaponController : MonoBehaviour {
        [SerializeField] Collider weaponCollider;

        void Awake() {
            SetWeaponColliderState(false);
        }

        public void SetWeaponColliderState(bool isActive) {
            weaponCollider.enabled = isActive;
        }
    }
}
