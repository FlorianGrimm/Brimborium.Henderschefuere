namespace Brimborium.Henderschefuere.Transport;
public class TunnelWebSocketConnectionListenerFactory : IConnectionListenerFactory, IConnectionListenerFactorySelector
{
    private readonly TunnelWebSocketOptions _options;

    public TunnelWebSocketConnectionListenerFactory(IOptions<TunnelWebSocketOptions> options)
    {
        _options = options.Value;
    }

    public bool CanBind(EndPoint endpoint)
    {
        return endpoint is UriEndpointWebSocket;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        if (endpoint is not UriEndpointWebSocket uriEndpointWebSocket)
        {
            throw new ArgumentException("Invalid endpoint type", nameof(endpoint));
        }
        else
        {
            return new(new TunnelWebSocketConnectionListener(_options, uriEndpointWebSocket));
        }
    }
}
