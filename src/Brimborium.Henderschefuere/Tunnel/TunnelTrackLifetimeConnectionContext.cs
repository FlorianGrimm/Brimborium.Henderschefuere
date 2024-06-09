namespace Brimborium.Henderschefuere.Tunnel;

/// <summary>
/// This exists solely to track the lifetime of the connection
/// </summary>
internal sealed class TunnelTrackLifetimeConnectionContext : ConnectionContext {

    private readonly ConnectionContext _Connection;
    private readonly TaskCompletionSource _ExecutionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TunnelTrackLifetimeConnectionContext(ConnectionContext connection) {
        this._Connection = connection;
    }

    public Task ExecutionTask => this._ExecutionTcs.Task;

    public override string ConnectionId {
        get => this._Connection.ConnectionId;
        set => this._Connection.ConnectionId = value;
    }

    public override IFeatureCollection Features => this._Connection.Features;

    public override IDictionary<object, object?> Items {
        get => this._Connection.Items;
        set => this._Connection.Items = value;
    }

    public override IDuplexPipe Transport {
        get => this._Connection.Transport;
        set => this._Connection.Transport = value;
    }

    public override EndPoint? LocalEndPoint {
        get => this._Connection.LocalEndPoint;
        set => this._Connection.LocalEndPoint = value;
    }

    public override EndPoint? RemoteEndPoint {
        get => this._Connection.RemoteEndPoint;
        set => this._Connection.RemoteEndPoint = value;
    }

    public override CancellationToken ConnectionClosed {
        get => this._Connection.ConnectionClosed;
        set => this._Connection.ConnectionClosed = value;
    }

    public override void Abort() {
        this._Connection.Abort();
    }

    public override void Abort(ConnectionAbortedException abortReason) {
        this._Connection.Abort(abortReason);
    }

    public override ValueTask DisposeAsync() {
        this._ExecutionTcs.TrySetResult();
        return this._Connection.DisposeAsync();
    }
}
//