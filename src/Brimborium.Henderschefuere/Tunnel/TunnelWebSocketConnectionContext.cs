namespace Brimborium.Henderschefuere.Tunnel;

public class WebSocketUriEndPoint(Uri uri, HfTunnelModel tunnel) : UriEndPoint(uri) {
    public HfTunnelModel Tunnel { get; } = tunnel;
}

internal sealed class TunnelWebSocketConnectionContext : HttpConnection {
    private readonly CancellationTokenSource _cts = new();
    private WebSocket? _underlyingWebSocket;

    private TunnelWebSocketConnectionContext(HttpConnectionOptions options) :
        base(options, null) {
    }

    public override CancellationToken ConnectionClosed {
        get => this._cts.Token;
        set { }
    }

    public override void Abort() {
        this._cts.Cancel();
        this._underlyingWebSocket?.Abort();
    }

    public override void Abort(ConnectionAbortedException abortReason) {
        this._cts.Cancel();
        this._underlyingWebSocket?.Abort();
    }

    public override ValueTask DisposeAsync() {
        // REVIEW: Why doesn't dispose just work?
        this.Abort();

        return base.DisposeAsync();
    }

    internal sealed class Factory : ITunnelConnectionListenerFactory {
        public Type GetEndPointType() => typeof(WebSocketUriEndPoint);

        public async ValueTask<ConnectionContext> ConnectAsync(TunnelUriEndPoint endPoint, CancellationToken cancellationToken = default) {
            ClientWebSocket? underlyingWebSocket = null;
            var options = new HttpConnectionOptions {
                Url = endPoint.Uri,
                Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets,
                SkipNegotiation = true,
                WebSocketFactory = async (context, cancellationToken) => {
                    underlyingWebSocket = new ClientWebSocket();
                    underlyingWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);
                    await underlyingWebSocket.ConnectAsync(context.Uri, cancellationToken);
                    return underlyingWebSocket;
                }
            };

            var connection = new TunnelWebSocketConnectionContext(options);
            await connection.StartAsync(TransferFormat.Binary, cancellationToken);
            connection._underlyingWebSocket = underlyingWebSocket;
            return connection;
        }
    }
}
