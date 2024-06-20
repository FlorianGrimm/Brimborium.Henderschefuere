namespace Brimborium.Henderschefuere.Transport;

public class TransportTunnelWebSocketOptions {
    public int MaxConnectionCount { get; set; } = 10;

    public Action<TunnelConfig, ClientWebSocket>? ConfigureClientWebSocket { get; set; }
}
