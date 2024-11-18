using UnityEngine;

namespace Attribute {
    public class Stamina : EnergyBase {
        [SerializeField] float energyRegenPerSecond = 5;
        [SerializeField] float regenTimeOut = 1;

        CountdownTimer _regenTimeOutTimer;
        void Start() {
            _regenTimeOutTimer = new CountdownTimer(regenTimeOut);
        }

        void Update() {
            _regenTimeOutTimer.Tick(Time.deltaTime);
            if (_regenTimeOutTimer.IsFinished) {
                Increase(energyRegenPerSecond * Time.deltaTime);
            }
        }

        public override void Decrease(float amount, Vector3? hitPosition = null) {
            _regenTimeOutTimer.Start();
            base.Decrease(amount, hitPosition);
        }
    }
}