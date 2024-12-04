using System;

namespace Behavior.Events.Interfaces {
    public interface IRequireAttributeEventChannel {
        void InitializeEnergyChannel(EnergyValueChanged energyValueChannel, ref Action allChannelsInitialized);
    }
}
