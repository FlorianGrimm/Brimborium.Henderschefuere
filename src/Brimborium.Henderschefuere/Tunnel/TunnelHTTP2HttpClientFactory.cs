// Licensed under the MIT License.
namespace Brimborium.Henderschefuere.Tunnel;

#warning TODO
internal sealed class TunnelHTTP2HttpClientFactory
    : ITransportHttpClientFactorySelector
    , IForwarderHttpClientFactory {
    private readonly TunnelConnectionChannelManager _tunnelConnectionChannelManager;
    private readonly ILogger _logger;

    public TunnelHTTP2HttpClientFactory(
        TunnelConnectionChannelManager tunnelConnectionChannelManager,
        ILogger<TunnelHTTP2HttpClientFactory> logger) {
        this._tunnelConnectionChannelManager = tunnelConnectionChannelManager;
        this._logger = logger;
    }

    public TransportMode GetTransportMode() => TransportMode.TunnelHTTP2;

    public int GetOrder() => 0;

    public IForwarderHttpClientFactory? GetForwarderHttpClientFactory(TransportMode transportMode, ForwarderHttpClientContext context)
        => this;

    public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context) {
        if (CanReuseOldClient(context)) {
            Log.ClientReused(_logger, context.ClusterId);
            return context.OldClient!;
        }

        var handler = new SocketsHttpHandler {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            EnableMultipleHttp2Connections = true,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15),

            // NOTE: MaxResponseHeadersLength = 64, which means up to 64 KB of headers are allowed by default as of .NET Core 3.1.
        };

        ConfigureHandler(context, handler);

        handler.ConnectCallback = async (context, cancellationToken) => {
            await Task.CompletedTask;
            var channelId = context.DnsEndPoint.Host;
            if (_tunnelConnectionChannelManager.TryGetConnectionChannel(channelId, out var tunnelConnectionChannels)) {
                var (requests, responses) = tunnelConnectionChannels;

                // Ask for a connection
                await requests.Writer.WriteAsync(0, cancellationToken);

                while (true) {
                    var stream = await responses.Reader.ReadAsync(cancellationToken);

                    if (stream is ICloseable c && c.IsClosed) {
                        // Ask for another connection
                        await requests.Writer.WriteAsync(0, cancellationToken);

                        continue;
                    }

                    return stream;
                }
            }
            throw new Exception("Not implemented");
        };

        Log.ClientCreated(_logger, context.ClusterId);

        return new HttpMessageInvoker(handler, disposeHandler: true);
    }

    /// <summary>
    /// Checks if the options have changed since the old client was created. If not then the
    /// old client will be re-used. Re-use can avoid the latency of creating new connections.
    /// </summary>
    private bool CanReuseOldClient(ForwarderHttpClientContext context) {
        return context.OldClient is not null && context.NewConfig == context.OldConfig;
    }

    /// <summary>
    /// Allows configuring the <see cref="SocketsHttpHandler"/> instance. The base implementation
    /// applies settings from <see cref="ForwarderHttpClientContext.NewConfig"/>.
    /// <see cref="SocketsHttpHandler.UseProxy"/>, <see cref="SocketsHttpHandler.AllowAutoRedirect"/>,
    /// <see cref="SocketsHttpHandler.AutomaticDecompression"/>, and <see cref="SocketsHttpHandler.UseCookies"/>
    /// are disabled prior to this call.
    /// </summary>
    private void ConfigureHandler(ForwarderHttpClientContext context, SocketsHttpHandler handler) {
        var newConfig = context.NewConfig;
        if (newConfig.SslProtocols.HasValue) {
            handler.SslOptions.EnabledSslProtocols = newConfig.SslProtocols.Value;
        }
        if (newConfig.MaxConnectionsPerServer is not null) {
            handler.MaxConnectionsPerServer = newConfig.MaxConnectionsPerServer.Value;
        }
        if (newConfig.DangerousAcceptAnyServerCertificate ?? false) {
            handler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
        }

        handler.EnableMultipleHttp2Connections = newConfig.EnableMultipleHttp2Connections.GetValueOrDefault(true);

        if (newConfig.RequestHeaderEncoding is not null) {
            var encoding = Encoding.GetEncoding(newConfig.RequestHeaderEncoding);
            handler.RequestHeaderEncodingSelector = (_, _) => encoding;
        }

        if (newConfig.ResponseHeaderEncoding is not null) {
            var encoding = Encoding.GetEncoding(newConfig.ResponseHeaderEncoding);
            handler.ResponseHeaderEncodingSelector = (_, _) => encoding;
        }

        var webProxy = TryCreateWebProxy(newConfig.WebProxy);
        if (webProxy is not null) {
            handler.Proxy = webProxy;
            handler.UseProxy = true;
        }
    }

    private static IWebProxy? TryCreateWebProxy(WebProxyConfig? webProxyConfig) {
        if (webProxyConfig is null || webProxyConfig.Address is null) {
            return null;
        }

        var webProxy = new WebProxy(webProxyConfig.Address);

        webProxy.UseDefaultCredentials = webProxyConfig.UseDefaultCredentials.GetValueOrDefault(false);
        webProxy.BypassProxyOnLocal = webProxyConfig.BypassOnLocal.GetValueOrDefault(false);

        return webProxy;
    }

    private static class Log {
        private static readonly Action<ILogger, string, Exception?> _clientCreated = LoggerMessage.Define<string>(
              LogLevel.Debug,
              EventIds.ClientCreated,
              "New client created for cluster '{clusterId}'.");

        private static readonly Action<ILogger, string, Exception?> _clientReused = LoggerMessage.Define<string>(
            LogLevel.Debug,
            EventIds.ClientReused,
            "Existing client reused for cluster '{clusterId}'.");

        public static void ClientCreated(ILogger logger, string clusterId) {
            _clientCreated(logger, clusterId, null);
        }

        public static void ClientReused(ILogger logger, string clusterId) {
            _clientReused(logger, clusterId, null);
        }
    }
}

internal sealed class TunnelHTTP2Route {
    private readonly TunnelConnectionChannels _tunnelConnectionChannels;
    private readonly ClusterState _clusterState;
    private readonly IHostApplicationLifetime _lifetime;

    public TunnelHTTP2Route(TunnelConnectionChannels tunnelConnectionChannels, ClusterState clusterState, IHostApplicationLifetime lifetime) {
        this._tunnelConnectionChannels = tunnelConnectionChannels;
        this._clusterState = clusterState;
        this._lifetime = lifetime;
    }

    public IEndpointConventionBuilder Map(IEndpointRouteBuilder routes) {
        var cfg = _clusterState.Model.Config;
        var path = $"_Tunnel/{cfg.ClusterId}";

        return routes.MapPost(path, (Delegate)handlePost);
    }

    private async Task<IResult> handlePost(HttpContext context) {
        if (context.Connection.ClientCertificate is null) {
            //return Results.BadRequest();
            System.Console.Out.WriteLine("context.Connection.ClientCertificate is null");
        }


        // HTTP/2 duplex stream
        if (context.Request.Protocol != HttpProtocol.Http2) {
            return Results.BadRequest();
        }

        var (requests, responses) = _tunnelConnectionChannels;

        await requests.Reader.ReadAsync(context.RequestAborted);

        var stream = new TunnelDuplexHttpStream(context);

        using var reg = _lifetime.ApplicationStopping.Register(() => stream.Abort());

        // Keep reusing this connection while, it's still open on the backend
        while (!context.RequestAborted.IsCancellationRequested) {
            // Make this connection available for requests
            await responses.Writer.WriteAsync(stream, context.RequestAborted);

            await stream.StreamCompleteTask;

            stream.Reset();
        }

        return Results.Empty;
    }
}