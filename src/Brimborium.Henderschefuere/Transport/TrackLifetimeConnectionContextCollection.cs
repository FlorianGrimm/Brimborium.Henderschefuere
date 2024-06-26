namespace Brimborium.Henderschefuere.Transport;

#if false
internal sealed class TrackLifetimeConnectionContextCollection {
    // is owned by the owner TunnelXyzConnectionListener
    private readonly SemaphoreSlim _connectionLock;
    private readonly ConcurrentDictionary<ConnectionContext, ConnectionContext> _connections;

    public TrackLifetimeConnectionContextCollection(ConcurrentDictionary<ConnectionContext, ConnectionContext> connections, SemaphoreSlim connectionLock) {
        _connections = connections;
        _connectionLock = connectionLock;
    }

    //internal void Add(TrackLifetimeConnectionContext connection)
    //{
    //    _connections.TryAdd(connection, connection);
    //}

    internal TrackLifetimeConnectionContext AddInnerConnection(ConnectionContext innerConnection) {
        var connection = new TrackLifetimeConnectionContext(innerConnection, this);

        // Track this connection lifetime
        _connections.TryAdd(connection, connection);

        return connection;
    }

    internal void Remove(TrackLifetimeConnectionContext connection) {
        if (_connections.TryRemove(connection, out _)) {
            _connectionLock.Release();
        }
    }
}
#else

internal interface ITrackLifetimeConnectionContext {
    void SetTracklifetime(TrackLifetimeConnectionContextCollection trackLifetimeConnectionContextCollection);
}

internal sealed class TrackLifetimeConnectionContextCollection {
    // is owned by the owner TunnelXyzConnectionListener
    private readonly SemaphoreSlim _connectionLock;
    private readonly ConcurrentDictionary<ConnectionContext, ConnectionContext> _connections;

    public TrackLifetimeConnectionContextCollection(ConcurrentDictionary<ConnectionContext, ConnectionContext> connections, SemaphoreSlim connectionLock) {
        _connections = connections;
        _connectionLock = connectionLock;
    }
    internal ConnectionContext AddInnerConnection(ConnectionContext connectionContext) {
        //var connection = new TrackLifetimeConnectionContext(innerConnection, this);

        // Track this connection lifetime
        var trackLifetimeConnectionContext = (ITrackLifetimeConnectionContext)connectionContext;
        if (_connections.TryAdd(connectionContext, connectionContext)) {
            trackLifetimeConnectionContext.SetTracklifetime(this);
        }

        return connectionContext;
    }

    internal void Remove(ConnectionContext connection) {
        if (_connections.TryRemove(connection, out _)) {
            _connectionLock.Release();
        }
    }
}
#endif
