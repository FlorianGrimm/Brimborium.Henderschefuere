// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Tunnel;

internal sealed class TunnelWebSocketRoute {
    private readonly UnShortCitcuitOnceProxyConfigManager _unShortCitcuitOnceProxyConfigManager;
    private readonly TunnelConnectionChannelManager _tunnelConnectionChannelManager;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public TunnelWebSocketRoute(
        UnShortCitcuitOnceProxyConfigManager unShortCitcuitOnceProxyConfigManager,
        TunnelConnectionChannelManager tunnelConnectionChannelManager,
        IHostApplicationLifetime lifetime,
        ILogger<TunnelWebSocketRoute> logger) {
        _unShortCitcuitOnceProxyConfigManager = unShortCitcuitOnceProxyConfigManager;
        _tunnelConnectionChannelManager = tunnelConnectionChannelManager;
        _lifetime = lifetime;
        _logger = logger;

        _lifetime.ApplicationStopping.Register(() => _cancellationTokenSource.Cancel());
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
        using (var ctsRequestAborted = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, _cancellationTokenSource.Token)) {
            var responsesWriter = responses.Writer;
            await requests.Reader.ReadAsync(ctsRequestAborted.Token);

            var ws = await context.WebSockets.AcceptWebSocketAsync();

            var stream = new TunnelWebSocketStream(ws);

            // We should make this more graceful
            //using var reg = _lifetime.ApplicationStopping.Register(() => stream.Abort());

            // Keep reusing this connection while, it's still open on the backend
            while (ws.State == WebSocketState.Open) {
                // Make this connection available for requests
                await responsesWriter.WriteAsync(stream, ctsRequestAborted.Token);

                await stream.StreamCompleteTask;

                stream.Reset();
            }
        }

        return Results.Empty;
    }

    private static class Log {
        private static readonly Action<ILogger, string, Exception?> _parameterNotValid = LoggerMessage.Define<string>(
            LogLevel.Warning,
            EventIds.ParameterNotValid,
            "Requiered Parameter {name} - value is not valid.");

        public static void ParameterNotValid(ILogger logger, string parameterName) {
            _parameterNotValid(logger, parameterName, null);
        }

        private static readonly Action<ILogger, string, Exception?> _clusterNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            EventIds.ClusterNotFound,
            "Cluster {name} not found.");

        public static void ClusterNotFound(ILogger logger, string parameterName) {
            _clusterNotFound(logger, parameterName, null);
        }

        private static readonly Action<ILogger, string, Exception?> _tunnelConnectionChannelNotFound = LoggerMessage.Define<string>(
            LogLevel.Warning,
            EventIds.TunnelConnectionChannelNotFound,
            "TunnelConnectionChannel {name} not found.");

        public static void TunnelConnectionChannelNotFound(ILogger logger, string parameterName) {
            _tunnelConnectionChannelNotFound(logger, parameterName, null);
        }

        /*
        private static readonly Action<ILogger, string, Exception?> _hugo = LoggerMessage.Define<string>(
            LogLevel.Warning,
            EventIds.ParameterNotValid,
            " {name} is not valid.");

        public static void Hugo(ILogger logger, string parameterName) {
            _hugo(logger, parameterName, null);
        }
        */
    }
}
