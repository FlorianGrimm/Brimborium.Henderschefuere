namespace Brimborium.Henderschefuere.Tunnel;

public sealed class TunnelConnectionListener : IConnectionListener {
    private readonly SemaphoreSlim _ConnectionLock;
    private readonly TunnelOptions _Options;
    private readonly TunnelUriEndPoint _Endpoint;
    private readonly ITunnelConnectionListenerFactory _TunnelConnectionListenerFactory;
    private readonly CancellationTokenSource _ClosedCts = new();
    private readonly ConcurrentDictionary<ConnectionContext, ConnectionContext> _Connections = new();

    public TunnelConnectionListener(
        TunnelOptions options,
        TunnelUriEndPoint endpoint,
        ITunnelConnectionListenerFactory tunnelConnectionListenerFactory) {
        this._Options = options;
        this._Endpoint = endpoint;
        this._TunnelConnectionListenerFactory = tunnelConnectionListenerFactory;
        this._ConnectionLock = new(options.MaxConnectionCount);
    }

    public EndPoint EndPoint => this._Endpoint;

    public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default) {
        try {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(this._ClosedCts.Token, cancellationToken).Token;

            // Kestrel will keep an active accept call open as long as the transport is active
            await this._ConnectionLock.WaitAsync(cancellationToken);

            while (true) {
                cancellationToken.ThrowIfCancellationRequested();

                try {
                    //this._Options.Transport switch {
                    //TransportType.WebSockets => await WebSocketConnectionContext.ConnectAsync(Uri, cancellationToken),
                    //TransportType.HTTP2 => await HttpClientConnectionContext.ConnectAsync(_httpMessageInvoker, Uri, cancellationToken),
                    //    _ => throw new NotSupportedException(),
                    //};
                    ConnectionContext connectionInner = await this._TunnelConnectionListenerFactory.ConnectAsync(this._Endpoint, cancellationToken);
                    if (connectionInner is TunnelTrackLifetimeConnectionContext connection) {
                        // Already a lifetime tracking connection
                    } else {
                        connection = new TunnelTrackLifetimeConnectionContext(connectionInner);
                    }

                    // Track this connection lifetime
                    this._Connections.TryAdd(connection, connection);

                    _ = Task.Run(async () => {
                        // When the connection is disposed, release it
                        try {
                            await connection.ExecutionTask;
                        } finally {
                            this._Connections.TryRemove(connection, out _);

                            // Allow more connections in
                            this._ConnectionLock.Release();
                        }
                    },
                    cancellationToken);

                    return connection;
                } catch (Exception ex) when (ex is not OperationCanceledException) {
                    // TODO: More sophisticated backoff and retry
                    await Task.Delay(5000, cancellationToken);
                }
            }
        } catch (OperationCanceledException) {
            return null;
        }
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default) {
        this._ClosedCts.Cancel();

        foreach (var (_, connection) in this._Connections) {
            connection.Abort();
        }

        return ValueTask.CompletedTask;
    }


    public async ValueTask DisposeAsync() {
        List<Task>? tasks = null;

        foreach (var (_, connection) in this._Connections) {
            tasks ??= new();
            tasks.Add(connection.DisposeAsync().AsTask());
        }

        if (tasks is null) {
            return;
        }

        await Task.WhenAll(tasks);
    }
}
