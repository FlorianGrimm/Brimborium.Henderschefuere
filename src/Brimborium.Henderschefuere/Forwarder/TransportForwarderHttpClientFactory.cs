// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Forwarder;

internal sealed class TransportForwarderHttpClientFactory : ITransportHttpClientFactorySelector {
    private readonly IForwarderHttpClientFactory _ForwarderHttpClientFactory;

    public TransportForwarderHttpClientFactory(
        IForwarderHttpClientFactory forwarderHttpClientFactory
        ) {
        this._ForwarderHttpClientFactory = forwarderHttpClientFactory;
    }

    public TransportMode GetTransportMode() => TransportMode.Forwarder;

    public int GetOrder() => 0;

    public IForwarderHttpClientFactory? GetForwarderHttpClientFactory(TransportMode transportMode, ForwarderHttpClientContext context) {
        return _ForwarderHttpClientFactory;
    }
}
