// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Tunnel;

#if false
internal sealed class TunnelWebSocketRoute {
    private readonly TunnelConnectionChannels _tunnelConnectionChannels;
    private readonly ClusterState _clusterState;
    private readonly IHostApplicationLifetime _lifetime;

    public TunnelWebSocketRoute(TunnelConnectionChannels tunnelConnectionChannels, ClusterState clusterState, IHostApplicationLifetime lifetime) {
        this._tunnelConnectionChannels = tunnelConnectionChannels;
        this._clusterState = clusterState;
        this._lifetime = lifetime;
    }

    public IEndpointConventionBuilder Map(IEndpointRouteBuilder routes) {
        var cfg = _clusterState.Model.Config;
        var path = $"_Tunnel/{cfg.ClusterId}";

        var conventionBuilder = routes.MapGet(path, (Delegate)handleGet);

        // Make this endpoint do websockets automagically as middleware for this specific route
        conventionBuilder.Add(e => {
            var sub = routes.CreateApplicationBuilder();
            sub.UseWebSockets().Run(e.RequestDelegate!);
            e.RequestDelegate = sub.Build();
        });

        return conventionBuilder;
    }

    private async Task<IResult> handleGet(HttpContext context) {
        if (context.Connection.ClientCertificate is null) {
            //return Results.BadRequest();
            System.Console.Out.WriteLine("context.Connection.ClientCertificate is null");
        }

        if (!context.WebSockets.IsWebSocketRequest) {
            return Results.BadRequest();
        }

        var (requests, responses) = _tunnelConnectionChannels;

        await requests.Reader.ReadAsync(context.RequestAborted);

        var ws = await context.WebSockets.AcceptWebSocketAsync();

        var stream = new TunnelWebSocketStream(ws);

        // We should make this more graceful
        using var reg = _lifetime.ApplicationStopping.Register(() => stream.Abort());

        // Keep reusing this connection while, it's still open on the backend
        while (ws.State == WebSocketState.Open) {
            // Make this connection available for requests
            await responses.Writer.WriteAsync(stream, context.RequestAborted);

            await stream.StreamCompleteTask;

            stream.Reset();
        }

        return Results.Empty;
    }
}
#else
internal sealed class TunnelWebSocketRoute {
    private readonly UnShortCitcuitOnceProxyConfigManager _unShortCitcuitOnceProxyConfigManager;
    private readonly TunnelConnectionChannelManager _tunnelConnectionChannelManager;
    private readonly IHostApplicationLifetime _lifetime;

    public TunnelWebSocketRoute(
        UnShortCitcuitOnceProxyConfigManager unShortCitcuitOnceProxyConfigManager,
        TunnelConnectionChannelManager tunnelConnectionChannelManager,
        IHostApplicationLifetime lifetime) {
        _unShortCitcuitOnceProxyConfigManager = unShortCitcuitOnceProxyConfigManager;
        _tunnelConnectionChannelManager = tunnelConnectionChannelManager;
        _lifetime = lifetime;
    }
    internal IEndpointConventionBuilder Map(IEndpointRouteBuilder endpoints) {
#pragma warning disable ASP0018 // Unused route parameter
        var conventionBuilder = endpoints.MapGet("_Tunnel/{clusterId}", (Delegate)TunnelWebSocketRouteGet);
#pragma warning restore ASP0018 // Unused route parameter
#warning TODO: builder.RequireAuthorization

        // Make this endpoint do websockets automagically as middleware for this specific route
        conventionBuilder.Add(e => {
            var sub = endpoints.CreateApplicationBuilder();
            sub.UseWebSockets().Run(e.RequestDelegate!);
            e.RequestDelegate = sub.Build();
        });

        return conventionBuilder;
    }

    private async Task<IResult> TunnelWebSocketRouteGet(HttpContext context) {
        if (context.GetRouteValue("clusterId") is not string clusterId) {
            // TODO: log
            return Results.BadRequest();
        }

        if (!context.WebSockets.IsWebSocketRequest) {
            return Results.BadRequest();
        }

        var proxyConfigManager = _unShortCitcuitOnceProxyConfigManager.GetService();
        if (!proxyConfigManager.TryGetCluster(clusterId, out var cluster)) {
            // TODO: log
            return Results.BadRequest();
        }

        if (!_tunnelConnectionChannelManager.TryGetConnectionChannel(clusterId, out var tunnelConnectionChannels)) {
            // TODO: log
            return Results.BadRequest();
        }


        var (requests, responses) = tunnelConnectionChannels;

        await requests.Reader.ReadAsync(context.RequestAborted);

        var ws = await context.WebSockets.AcceptWebSocketAsync();

        var stream = new TunnelWebSocketStream(ws);

        // We should make this more graceful
        using var reg = _lifetime.ApplicationStopping.Register(() => stream.Abort());

        // Keep reusing this connection while, it's still open on the backend
        while (ws.State == WebSocketState.Open) {
            // Make this connection available for requests
            await responses.Writer.WriteAsync(stream, context.RequestAborted);

            await stream.StreamCompleteTask;

            stream.Reset();
        }

        return Results.Empty;
    }
    internal void Register(IEnumerable<ClusterState> tunnelClusters) {
        foreach (var cluster in tunnelClusters) {
            if (cluster.Model.Config.Transport == TransportMode.TunnelHTTP2) {
                _tunnelConnectionChannelManager.RegisterConnectionChannel(cluster.ClusterId);
            }
        }
    }

}
#endif