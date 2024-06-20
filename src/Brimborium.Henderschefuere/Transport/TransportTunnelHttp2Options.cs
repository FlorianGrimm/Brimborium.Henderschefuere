namespace Brimborium.Henderschefuere.Transport;

public sealed class TransportTunnelHttp2Options {
    public int MaxConnectionCount { get; set; } = 10;

    /// <summary>
    /// Authentification for the tunnel
    /// </summary>
    public Func<Uri, TunnelConfig, SocketsHttpHandler, ValueTask<HttpMessageHandler>>? ConfigureSocketsHttpHandlerAsync { get; set; }

    /// <summary>
    /// Authentification for the tunnel
    /// </summary>
    public Func<Uri, TunnelConfig, HttpRequestMessage, ValueTask<HttpRequestMessage>>? ConfigureHttpRequestMessageAsync { get; set; }
}
