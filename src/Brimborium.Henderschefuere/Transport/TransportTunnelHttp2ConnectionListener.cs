namespace Brimborium.Henderschefuere.Transport;

/// <summary>
/// This has the core logic that creates and maintains connections to the proxy.
/// </summary>
internal sealed class TransportTunnelHttp2ConnectionListener : IConnectionListener {
    private readonly SemaphoreSlim _createHttpMessageInvokerLock;
    private readonly SemaphoreSlim _connectionLock;
    private readonly ConcurrentDictionary<ConnectionContext, ConnectionContext> _connections = new();
    private readonly TrackLifetimeConnectionContextCollection _connectionCollection;
    private readonly CancellationTokenSource _closedCts = new();
    private readonly TransportTunnelHttp2Options _options;
    private readonly TunnelState _tunnel;
    private readonly UriEndPointHttp2 _endPoint;
    private readonly IncrementalDelay _delay = new();

    private HttpMessageInvoker? _httpMessageInvoker;

    public TransportTunnelHttp2ConnectionListener(
        TransportTunnelHttp2Options options,
        TunnelState tunnel,
        UriEndPointHttp2 endpoint) {
        if (string.IsNullOrEmpty(endpoint.Uri?.ToString())) {
            throw new ArgumentException("UriEndPoint.Uri is required", nameof(endpoint));
        }
        _createHttpMessageInvokerLock = new(1, 1);
        _connectionLock = new(options.MaxConnectionCount);
        _connectionCollection = new TrackLifetimeConnectionContextCollection(_connections, _connectionLock);
        _options = options;
        _tunnel = tunnel;
        _endPoint = endpoint;
    }

    public EndPoint EndPoint => _endPoint;

    public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default) {
        try {
            cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_closedCts.Token, cancellationToken).Token;

            // Kestrel will keep an active accept call open as long as the transport is active
            await _connectionLock.WaitAsync(cancellationToken);


            if (_httpMessageInvoker is null) {
                await _createHttpMessageInvokerLock.WaitAsync(cancellationToken);
                _httpMessageInvoker ??= await CreateHttpMessageInvoker();
                _createHttpMessageInvokerLock.Release();
            }


            while (true) {
                cancellationToken.ThrowIfCancellationRequested();

                HttpRequestMessage requestMessage = new HttpRequestMessage(
                    HttpMethod.Post, _endPoint.Uri!) {
                    Version = new Version(2, 0)
                };
                if (_options.ConfigureHttpRequestMessageAsync is { } configure) {
                    requestMessage = await configure(_endPoint.Uri!, this._tunnel.Model.Config, requestMessage);
                }

                try {
                    var innerConnection = await TransportTunnelHttp2ConnectionContext.ConnectAsync(
                        requestMessage, _httpMessageInvoker, cancellationToken);
                    _delay.Reset();
                    return _connectionCollection.AddInnerConnection(innerConnection);
                } catch (Exception ex) when (ex is not OperationCanceledException) {
                    requestMessage?.Dispose();
                    // TODO: More sophisticated backoff and retry
                    await _delay.Delay(cancellationToken);
                }
            }
        } catch (OperationCanceledException) {
            return null;
        }
    }

    private async Task<HttpMessageInvoker> CreateHttpMessageInvoker() {
        var socketsHttpHandler = new SocketsHttpHandler {
            EnableMultipleHttp2Connections = true,
            PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        };

        //socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

#warning TODO HERE cert x(socketsHttpHandler, _tunnel.Model.Config.Authentication);

        if (_options.ConfigureSocketsHttpHandlerAsync is { } configure) {
            var httpMessageHandler = await configure(_endPoint.Uri!, _tunnel.Model.Config, socketsHttpHandler);
            return new HttpMessageInvoker(httpMessageHandler);
        } else {
            return new HttpMessageInvoker(socketsHttpHandler);
        }
    }

    public async ValueTask DisposeAsync() {
        var listConnections = _connections.Values.ToList();
        List<Task> tasks = new(listConnections.Count);
        foreach (var connection in listConnections) {
            tasks.Add(connection.DisposeAsync().AsTask());
        }

        if (tasks.Count > 0) {
            await Task.WhenAll(tasks);
        }
    }

    public ValueTask UnbindAsync(CancellationToken cancellationToken = default) {
        _closedCts.Cancel();

        var listConnections = _connections.Values.ToList();
        foreach (var connection in listConnections) {
            // REVIEW: Graceful?
            connection.Abort();
        }

        return ValueTask.CompletedTask;
    }
}
