using UnityEngine;

namespace Effects {
    [CreateAssetMenu(menuName = "Effects/Audio", fileName = "AudioEffect")]
    public class AudioEffect : GenericEffect<AudioClip> { }
    
    [CreateAssetMenu(menuName = "Effects/Particle", fileName = "ParticleEffect")]
    public class ParticleEffect : GenericEffect<GameObject> { }
}