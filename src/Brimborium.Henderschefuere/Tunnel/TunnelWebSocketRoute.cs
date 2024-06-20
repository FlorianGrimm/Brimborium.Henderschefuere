// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Tunnel;

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
