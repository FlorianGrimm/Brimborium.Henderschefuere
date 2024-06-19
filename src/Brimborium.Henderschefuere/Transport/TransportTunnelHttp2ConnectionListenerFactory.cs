namespace Brimborium.Henderschefuere.Transport;

public class TransportTunnelHttp2ConnectionListenerFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector {
    private readonly TransportTunnelHttp2Options _options;

    public TransportTunnelHttp2ConnectionListenerFactory(IOptions<TransportTunnelHttp2Options> options) {
        _options = options.Value;
    }

    public bool CanBind(EndPoint endpoint) {
        return endpoint is UriEndPointHttp2;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is not UriEndPointHttp2 uriEndPointHttp2) {
            throw new ArgumentException("Invalid endpoint type", nameof(endpoint));
        } else {
            return new(new TransportTunnelHttp2ConnectionListener(_options, uriEndPointHttp2));
        }
    }
}
