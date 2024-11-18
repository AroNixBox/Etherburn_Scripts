using UnityEngine;

namespace Attribute {
    public class Hunger : EnergyBase {
        [SerializeField] float hungerDecreaseTimeout = 1f;
        [SerializeField] float hungerUsagePerSecond = 2.5f;
        
        CountdownTimer _healthDecreaseTimeoutTimer;

        public void Start() {
            _healthDecreaseTimeoutTimer = new CountdownTimer(hungerDecreaseTimeout);
        }

        void Update() {
            _healthDecreaseTimeoutTimer.Tick(Time.deltaTime);
            if (_healthDecreaseTimeoutTimer.IsFinished) {
                Decrease(hungerUsagePerSecond * Time.deltaTime);
            }
        }

        public override void Increase(float amount) {
            _healthDecreaseTimeoutTimer.Start();
            base.Increase(amount);
        }
    }
}