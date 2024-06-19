// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Model;

public sealed class TunnelState {
    private TunnelModel _Model = null!;

    public TunnelState(string tunnelId) {
        this.TunnelId = tunnelId;
    }

    public string TunnelId { get; }

    public TunnelModel Model { get => _Model; internal set => _Model = value; }
}
