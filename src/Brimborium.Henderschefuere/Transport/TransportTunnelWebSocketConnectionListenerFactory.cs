namespace Brimborium.Henderschefuere.Transport;

public class TransportTunnelWebSocketConnectionListenerFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector {
    private readonly TransportTunnelWebSocketOptions _options;

    public TransportTunnelWebSocketConnectionListenerFactory(IOptions<TransportTunnelWebSocketOptions> options) {
        _options = options.Value;
    }

    public bool CanBind(EndPoint endpoint) {
        return endpoint is UriWebSocketEndPoint;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is not UriWebSocketEndPoint uriEndpointWebSocket) {
            throw new ArgumentException("Invalid endpoint type", nameof(endpoint));
        } else {
            return new(new TransportTunnelWebSocketConnectionListener(_options, uriEndpointWebSocket));
        }
    }
}
