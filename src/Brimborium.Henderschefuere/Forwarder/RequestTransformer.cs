// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Forwarder;

internal sealed class RequestTransformer : HttpTransformer {
    private readonly Func<HttpContext, HttpRequestMessage, ValueTask> _requestTransform;

    public RequestTransformer(Func<HttpContext, HttpRequestMessage, ValueTask> requestTransform) {
        _requestTransform = requestTransform;
    }

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken) {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        await _requestTransform(httpContext, proxyRequest);
    }
}
