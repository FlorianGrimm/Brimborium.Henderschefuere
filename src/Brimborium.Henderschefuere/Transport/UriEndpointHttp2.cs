namespace Brimborium.Henderschefuere.Transport;

public sealed class UriEndPointHttp2 : IPEndPoint {
    public Uri? Uri { get; }
    public string? TunnelId { get; }

    public UriEndPointHttp2(Uri uri, string tunnelId)
        : this(0, 0) {
        Uri = uri;
        this.TunnelId = tunnelId;
    }

    public UriEndPointHttp2(long address, int port) : base(address, port) {
    }
}
