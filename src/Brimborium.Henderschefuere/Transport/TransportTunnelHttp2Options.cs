namespace Brimborium.Henderschefuere.Transport;

public class TransportTunnelHttp2Options {
    public int MaxConnectionCount { get; set; } = 10;

    public Action<Uri, SocketsHttpHandler>? ConfigureSocketsHttpHandler { get; set; }
}
