namespace Brimborium.Henderschefuere.Transport;

public class TransportTunnelWebSocketOptions {
    public int MaxConnectionCount { get; set; } = 10;

    public Action<Uri, ClientWebSocket>? ConfigureClientWebSocket { get; set; }
}
