namespace Brimborium.Henderschefuere.Transport;

public sealed class UriWebSocketEndPoint : IPEndPoint {
    public Uri? Uri { get; }
    public string? TunnelId { get; }

    public UriWebSocketEndPoint(Uri uri, string tunnelId)
        : this(0, 0) {
        Uri = uri;
        this.TunnelId = tunnelId;
    }

    public UriWebSocketEndPoint(long address, int port) : base(address, port) {
    }
}
