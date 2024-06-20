using Microsoft.AspNetCore.Http.Connections;

namespace Brimborium.Henderschefuere.Transport;

internal sealed class TransportTunnelWebSocketConnectionContext : HttpConnection {
    private readonly CancellationTokenSource _cts = new();
    private WebSocket? _underlyingWebSocket;

    private TransportTunnelWebSocketConnectionContext(HttpConnectionOptions options)
        : base(options, null) {
    }

    public override CancellationToken ConnectionClosed {
        get => _cts.Token;
        set { }
    }

    public override void Abort() {
        _cts.Cancel();
        _underlyingWebSocket?.Abort();
    }

    public override void Abort(ConnectionAbortedException abortReason) {
        _cts.Cancel();
        _underlyingWebSocket?.Abort();
    }

    public override ValueTask DisposeAsync() {
        // REVIEW: Why doesn't dispose just work?
        Abort();

        return base.DisposeAsync();
    }

    internal static async ValueTask<TransportTunnelWebSocketConnectionContext> ConnectAsync(
        Uri uri,
        Action<ClientWebSocket> configureClientWebSocket,
        CancellationToken cancellationToken) {
        ClientWebSocket? underlyingWebSocket = null;
        var options = new HttpConnectionOptions {
            Url = uri,
            Transports = HttpTransportType.WebSockets,
            SkipNegotiation = true,
            WebSocketFactory = async (context, cancellationToken) => {
                underlyingWebSocket = new ClientWebSocket();
                underlyingWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

                // underlyingWebSocket.Options.ClientCertificates

                if (configureClientWebSocket is not null) {
                    configureClientWebSocket(underlyingWebSocket);
                }
                await underlyingWebSocket.ConnectAsync(context.Uri, cancellationToken);
                return underlyingWebSocket;
            }
        };

        var connection = new TransportTunnelWebSocketConnectionContext(options);
        await connection.StartAsync(TransferFormat.Binary, cancellationToken);
        connection._underlyingWebSocket = underlyingWebSocket;
        return connection;
    }
}
