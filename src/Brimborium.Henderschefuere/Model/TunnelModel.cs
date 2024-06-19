// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Model;

public sealed class TunnelModel {
    public TunnelModel(TunnelConfig config) {
        this.Config = config;
    }

    public TunnelConfig Config { get; }

    internal bool HasConfigChanged(TunnelModel newModel) {
        return !Config.Equals(newModel.Config);
    }
}