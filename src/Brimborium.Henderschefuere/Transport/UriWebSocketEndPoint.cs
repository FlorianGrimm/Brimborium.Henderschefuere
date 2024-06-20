namespace Brimborium.Henderschefuere.Transport;

public sealed class UriWebSocketEndPoint(
        Uri uri,
        string tunnelId
    ) : IPEndPoint(0, 0) {
    public Uri Uri { get; } = uri;
    public string TunnelId { get; } = tunnelId;

    public override string ToString() => $"{Uri}#{TunnelId}";
}
