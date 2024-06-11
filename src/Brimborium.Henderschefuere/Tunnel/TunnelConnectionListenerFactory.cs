namespace Brimborium.Henderschefuere.Tunnel;

public class TunnelUriEndPoint(Uri uri, HfTunnelModel tunnel) : UriEndPoint(uri) {
    public HfTunnelModel Tunnel { get; } = tunnel;
}

/// <summary>
/// Represents a factory for creating tunnel connection listeners.
/// </summary>
public interface ITunnelConnectionListenerFactory {
    /// <summary>
    /// Gets the type of endpoint that the factory supports.
    /// </summary>
    /// <returns></returns>
    Type GetEndPointType();

    /// <summary>
    /// Connects to the specified URI.
    /// </summary>
    /// <param name="endPoint">the uri</param>
    /// <param name="cancellationToken">stop</param>
    /// <returns>the ConnectionContext - async.</returns>
    ValueTask<ConnectionContext> ConnectAsync(TunnelUriEndPoint endPoint, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a factory for creating tunnel connection listeners.
/// </summary>
public sealed class TunnelConnectionListenerFactory
    : IConnectionListenerFactory
    , IConnectionListenerFactorySelector {
    public static ImmutableDictionary<Type, ITunnelConnectionListenerFactory> GetTunnelConnectionListenerFactoryByType(List<ITunnelConnectionListenerFactory> listTunnelConnectionListenerFactory) {
        Dictionary<Type, ITunnelConnectionListenerFactory> dict = new();
        foreach (var item in listTunnelConnectionListenerFactory) {
            dict.Add(item.GetEndPointType(), item);
        }
        var result = dict.ToImmutableDictionary();
        return result;
    }

    private readonly TunnelOptions _options;
    private readonly ImmutableDictionary<Type, ITunnelConnectionListenerFactory> _TunnelConnectionListenerFactoryByType;

    public TunnelConnectionListenerFactory(
        List<ITunnelConnectionListenerFactory> listTunnelConnectionListenerFactory,
        IOptions<TunnelOptions> options) {
        this._options = options.Value;
        this._TunnelConnectionListenerFactoryByType = GetTunnelConnectionListenerFactoryByType(listTunnelConnectionListenerFactory);
    }

    /// <summary>
    /// Determines whether the factory can bind to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to bind to.</param>
    /// <returns><c>true</c> if the factory can bind to the endpoint; otherwise, <c>false</c>.</returns>
    public bool CanBind(EndPoint endpoint) {
        return this._TunnelConnectionListenerFactoryByType.ContainsKey(endpoint.GetType());
    }

    /// <summary>
    /// Binds to the specified endpoint and returns a connection listener.
    /// </summary>
    /// <param name="endpoint">The endpoint to bind to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.</returns>
    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is TunnelUriEndPoint tunnelUriEndPoint
            && this._TunnelConnectionListenerFactoryByType.TryGetValue(endpoint.GetType(), out var factory)) {
            return new(new TunnelConnectionListener(this._options, tunnelUriEndPoint, factory));
        } else {
            throw new NotSupportedException();
        }
    }
}

public class TunnelOptions {
    public int MaxConnectionCount { get; set; } = 10;
}

public enum TransportType {
    WebSockets,
    HTTP2
}
