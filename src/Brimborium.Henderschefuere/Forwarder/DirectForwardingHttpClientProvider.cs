// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Forwarder;

internal sealed class DirectForwardingHttpClientProvider
{
    public HttpMessageInvoker HttpClient { get; }

    public DirectForwardingHttpClientProvider() : this(new ForwarderHttpClientFactory()) { }

    public DirectForwardingHttpClientProvider(IForwarderHttpClientFactory factory)
    {
        HttpClient = factory.CreateClient(new ForwarderHttpClientContext
        {
            NewConfig = HttpClientConfig.Empty
        });
    }
}
