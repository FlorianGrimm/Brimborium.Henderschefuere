// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Tunnel;

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
#warning TODO: authn not here...
        //if (context.Connection.ClientCertificate is null) {
        //    //return Results.BadRequest();
        //    //System.Console.Out.WriteLine("context.Connection.ClientCertificate is null");
        //}


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