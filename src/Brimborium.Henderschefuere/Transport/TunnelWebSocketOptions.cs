namespace Brimborium.Henderschefuere.Transport;

public class TunnelWebSocketOptions {
    public int MaxConnectionCount { get; set; } = 10;

    public Action<Uri, ClientWebSocket>? ConfigureClientWebSocket { get; set; }
}
