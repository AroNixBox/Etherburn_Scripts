using UnityEngine;

namespace Player.Audio {
    [CreateAssetMenu(fileName = "PlayerSounds", menuName = "Player/PlayerSounds")]
    public class PlayerSounds : ScriptableObject {
        public AudioClip[] hurtSounds;
        public AudioClip[] deathSounds;
    }
}
