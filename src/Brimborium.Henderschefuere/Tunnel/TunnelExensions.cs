namespace Brimborium.Henderschefuere.Tunnel;
public static class TunnelExensions {





    public static IEndpointConventionBuilder MapHttp2Tunnel(this IEndpointRouteBuilder routes, ClusterState clusterState) {
        var cfg = clusterState.Model.Config;
        var tunnelClientFactory = routes.ServiceProvider.GetRequiredService<TunnelConnectionChannelManager>();
        if (!(tunnelClientFactory.RegisterConnectionChannel(cfg.ClusterId) is { } tunnelConnectionChannels)) {
            throw new Exception("ClusterId already exists.");
        }
        var lifetime = routes.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        var route = new TunnelHTTP2Route(tunnelConnectionChannels, clusterState, lifetime);
        var result = route.Map(routes);
        return result;
    }

    public static IEndpointConventionBuilder MapWebSocketTunnel(this IEndpointRouteBuilder routes, ClusterState clusterState) {
        var cfg = clusterState.Model.Config;
        var tunnelClientFactory = routes.ServiceProvider.GetRequiredService<TunnelConnectionChannelManager>();
        if (!(tunnelClientFactory.RegisterConnectionChannel(cfg.ClusterId) is { } tunnelConnectionChannels)) {
            throw new Exception("ClusterId already exists.");
        }
        var lifetime = routes.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        var route = new TunnelWebSocketRoute(tunnelConnectionChannels, clusterState, lifetime);
        return route.Map(routes);
    }

    // backend
    public static IReverseProxyBuilder UseTunnelTransport(
        this IReverseProxyBuilder builder,
        WebApplicationBuilder webApplicationBuilder,
        Action<TransportTunnelHttp2Options>? configureTunnelHttp2 = null,
        Action<TransportTunnelWebSocketOptions>? configureTunnelWebSocket = null
        ) {
        builder.Services.AddSingleton<IConnectionListenerFactory, TransportTunnelHttp2ConnectionListenerFactory>();
        builder.Services.AddSingleton<IConnectionListenerFactory, TransportTunnelWebSocketConnectionListenerFactory>();

        if (configureTunnelHttp2 is not null) {
            builder.Services.Configure(configureTunnelHttp2);
        }

        if (configureTunnelWebSocket is not null) {
            builder.Services.Configure(configureTunnelWebSocket);
        }

        webApplicationBuilder.WebHost.ConfigureKestrel(options => {
            var proxyConfigManager = options.ApplicationServices.GetRequiredService<ProxyConfigManager>();
            var tunnels = proxyConfigManager.GetTransportTunnels();
            foreach (var tunnel in tunnels) {
                var cfg = tunnel.Model.Config;
                var remoteTunnelId = cfg.GetRemoteTunnelId();
                var host = cfg.Url.TrimEnd('/');

                var uriTunnel = new Uri($"{host}/_Tunnel/{remoteTunnelId}");
                var transport = cfg.Transport;
                if (transport == TransportMode.TunnelHTTP2) {
                    options.Listen(new UriEndPointHttp2(uriTunnel, tunnel.TunnelId));
                    continue;
                }
                if (transport == TransportMode.TunnelWebSocket) {
                    options.Listen(new UriWebSocketEndPoint(uriTunnel, tunnel.TunnelId));
                    continue;
                }
            }
        });
        return builder;
    }
#if weichei
    [RequiresUnreferencedCode("i dont know how")]
    public static IEndpointConventionBuilder MapHttp2Tunnel(this IEndpointRouteBuilder routes, string path) {
        return routes.MapPost(path, static async (HttpContext context, string host, TunnelClientFactory tunnelFactory, IHostApplicationLifetime lifetime) => {
            if (context.Connection.ClientCertificate is null) {
                //return Results.BadRequest();
                System.Console.Out.WriteLine("context.Connection.ClientCertificate is null");
            }


            // HTTP/2 duplex stream
            if (context.Request.Protocol != HttpProtocol.Http2) {
                return Results.BadRequest();
            }

            var (requests, responses) = tunnelFactory.GetConnectionChannel(host);

            await requests.Reader.ReadAsync(context.RequestAborted);

            var stream = new TunnelDuplexHttpStream(context);

            using var reg = lifetime.ApplicationStopping.Register(() => stream.Abort());

            // Keep reusing this connection while, it's still open on the backend
            while (!context.RequestAborted.IsCancellationRequested) {
                // Make this connection available for requests
                await responses.Writer.WriteAsync(stream, context.RequestAborted);

                await stream.StreamCompleteTask;

                stream.Reset();
            }

            return EmptyResult.Instance;
        });
    }

    [RequiresUnreferencedCode("i dont know how")]
    public static IEndpointConventionBuilder MapWebSocketTunnel(this IEndpointRouteBuilder routes, string path) {
        var conventionBuilder = routes.MapGet(path, static async (HttpContext context, string host, TunnelClientFactory tunnelFactory, IHostApplicationLifetime lifetime) => {
            if (!context.WebSockets.IsWebSocketRequest) {
                return Results.BadRequest();
            }

            var (requests, responses) = tunnelFactory.GetConnectionChannel(host);

            await requests.Reader.ReadAsync(context.RequestAborted);

            var ws = await context.WebSockets.AcceptWebSocketAsync();

            var stream = new TunnelWebSocketStream(ws);

            // We should make this more graceful
            using var reg = lifetime.ApplicationStopping.Register(() => stream.Abort());

            // Keep reusing this connection while, it's still open on the backend
            while (ws.State == WebSocketState.Open) {
                // Make this connection available for requests
                await responses.Writer.WriteAsync(stream, context.RequestAborted);

                await stream.StreamCompleteTask;

                stream.Reset();
            }

            return EmptyResult.Instance;
        });

        // Make this endpoint do websockets automagically as middleware for this specific route
        conventionBuilder.Add(e => {
            var sub = routes.CreateApplicationBuilder();
            sub.UseWebSockets().Run(e.RequestDelegate!);
            e.RequestDelegate = sub.Build();
        });

        return conventionBuilder;
    }

    // This is for .NET 6, .NET 7 has Results.Empty
    internal sealed class EmptyResult : IResult {
        internal static readonly EmptyResult Instance = new();

        public Task ExecuteAsync(HttpContext httpContext) {
            return Task.CompletedTask;
        }
    }
#endif
}
