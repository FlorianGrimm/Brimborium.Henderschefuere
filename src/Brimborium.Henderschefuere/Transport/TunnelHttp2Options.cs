namespace Brimborium.Henderschefuere.Transport;

public class TunnelHttp2Options
{
    public int MaxConnectionCount { get; set; } = 10;

    public Action<Uri, SocketsHttpHandler>? ConfigureSocketsHttpHandler { get; set; }
}
