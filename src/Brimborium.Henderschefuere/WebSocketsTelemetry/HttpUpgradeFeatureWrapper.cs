// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

using Microsoft.Net.Http.Headers;

namespace Brimborium.Henderschefuere.WebSocketsTelemetry;

internal sealed class HttpUpgradeFeatureWrapper : IHttpUpgradeFeature
{
    private readonly TimeProvider _timeProvider;

    public HttpContext HttpContext { get; private set; }

    public IHttpUpgradeFeature InnerUpgradeFeature { get; private set; }

    public WebSocketsTelemetryStream? TelemetryStream { get; private set; }

    public bool IsUpgradableRequest => InnerUpgradeFeature.IsUpgradableRequest;

    public HttpUpgradeFeatureWrapper(TimeProvider timeProvider, HttpContext httpContext, IHttpUpgradeFeature upgradeFeature)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        InnerUpgradeFeature = upgradeFeature ?? throw new ArgumentNullException(nameof(upgradeFeature));
    }

    public async Task<Stream> UpgradeAsync()
    {
        Debug.Assert(TelemetryStream is null);
        var opaqueTransport = await InnerUpgradeFeature.UpgradeAsync();

        if (HttpContext.Response.Headers.TryGetValue(HeaderNames.Upgrade, out var upgradeValues) &&
            upgradeValues.Count == 1 &&
            string.Equals("WebSocket", upgradeValues.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            TelemetryStream = new WebSocketsTelemetryStream(_timeProvider, opaqueTransport);
        }

        return TelemetryStream ?? opaqueTransport;
    }
}
