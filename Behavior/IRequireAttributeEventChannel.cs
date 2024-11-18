using System;

namespace Behavior {
    public interface IRequireAttributeEventChannel {
        void InitializeEnergyChannel(EnergyValueChanged energyValueChannel, ref Action allChannelsInitialized);
    }
}
