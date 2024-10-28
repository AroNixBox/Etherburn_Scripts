using UnityEngine;

namespace Attributes {
    public class Hunger : EnergyBase {
        [SerializeField] float hungerDecreaseTimeout = 1f;
        [SerializeField] float hungerUsagePerSecond = 2.5f;
        
        CountdownTimer _healthDecreaseTimeoutTimer;
        void Awake() {
            CurrentEnergy = maxEnergy;
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
        
        protected override string GetBlackboardVariableName() {
            return "OnHungerChangedEvent";
        }
    }
}