using UnityEngine;

namespace Effects.VFX {
    public class DecalGroundSpawner : MonoBehaviour {
        [SerializeField] ParticleSystem decalParticles;
        [SerializeField] float timeUltilDecalSpawn = 0.5f;
        CountdownTimer _timer;

        void Start() {
            if(decalParticles == null) {
                Debug.LogError("Decal Particles not set in " + name, gameObject);
                return;
            }
            _timer = new CountdownTimer(timeUltilDecalSpawn);
            _timer.OnTimerStop += SpawnDecal;
            _timer.Start();
        }
        
        void Update() {
            if(_timer == null) return;
            
            _timer.Tick(Time.deltaTime);
        }
        
        void SpawnDecal() {
            var rayCastResults = new RaycastHit[10];
            int hitCount = Physics.RaycastNonAlloc(transform.position, Vector3.down, rayCastResults);

            if (hitCount > 0) {
                for (int i = 0; i < hitCount; i++) {
                    var hit = rayCastResults[i];

                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.9f) {
                        Quaternion decalRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        var hitPointAbove = hit.point + Vector3.up * 0.05f;
                        Instantiate(decalParticles, hitPointAbove, decalRotation);

                        return;
                    }
                }
            }
        }


    }
}