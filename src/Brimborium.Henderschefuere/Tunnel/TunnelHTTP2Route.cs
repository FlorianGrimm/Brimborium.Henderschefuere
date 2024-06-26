﻿// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Tunnel;

internal sealed class TunnelHTTP2Route {
    private readonly UnShortCitcuitOnceProxyConfigManager _unShortCitcuitOnceProxyConfigManager;
    private readonly TunnelConnectionChannelManager _tunnelConnectionChannelManager;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public TunnelHTTP2Route(
        UnShortCitcuitOnceProxyConfigManager unShortCitcuitOnceProxyConfigManager,
        TunnelConnectionChannelManager tunnelConnectionChannelManager,
        IHostApplicationLifetime lifetime,
        ILogger<TunnelHTTP2Route> logger) {
        _unShortCitcuitOnceProxyConfigManager = unShortCitcuitOnceProxyConfigManager;
        _tunnelConnectionChannelManager = tunnelConnectionChannelManager;
        _lifetime = lifetime;
        _logger = logger;

        _lifetime.ApplicationStopping.Register(() => _cancellationTokenSource.Cancel());
    }

    internal IEndpointConventionBuilder Map(IEndpointRouteBuilder endpoints) {
#pragma warning disable ASP0018 // Unused route parameter
        var conventionBuilder = endpoints.MapPost("_Tunnel/{clusterId}", (Delegate)TunnelHTTP2RoutePost);
#pragma warning restore ASP0018 // Unused route parameter
#warning TODO: conventionBuilder.RequireAuthorization
        return conventionBuilder;
    }

    private async Task<IResult> TunnelHTTP2RoutePost(HttpContext context) {
        if (context.GetRouteValue("clusterId") is not string clusterId) {
            // TODO: log
            Log.ParameterNotValid(_logger, "Cluster");
            return Results.BadRequest();
        }

        // HTTP/2 duplex stream
        if (context.Request.Protocol != HttpProtocol.Http2) {
            return Results.BadRequest();
        }

        var proxyConfigManager = _unShortCitcuitOnceProxyConfigManager.GetService();
        if (!proxyConfigManager.TryGetCluster(clusterId, out var cluster)) {
            Log.ClusterNotFound(_logger, clusterId);
            return Results.BadRequest();
        }

        if (!_tunnelConnectionChannelManager.TryGetConnectionChannel(clusterId, out var tunnelConnectionChannels)) {
            Log.TunnelConnectionChannelNotFound(_logger, clusterId);
            // TODO: log
            return Results.BadRequest();
        }

        using (var ctsRequestAborted = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, _cancellationTokenSource.Token)) {

            var (requests, responses) = tunnelConnectionChannels;

            System.Threading.Interlocked.Increment(ref tunnelConnectionChannels.CountSource);
            var requestsReader = requests.Reader;
            var responsesWriter = responses.Writer;
            try {
                await requestsReader.ReadAsync(ctsRequestAborted.Token);

                using (var stream = new TunnelDuplexHttpStream(context)) {

                    using var reg = ctsRequestAborted.Token.Register(() => stream.Abort());

                    // Keep reusing this connection while, it's still open on the backend
                    while (!ctsRequestAborted.IsCancellationRequested) {
                        // Make this connection available for requests
                        await responsesWriter.WriteAsync(stream, ctsRequestAborted.Token);

                        await stream.StreamCompleteTask;

                        stream.Reset();

                        break;
                    }
                }
            } finally {
                System.Threading.Interlocked.Decrement(ref tunnelConnectionChannels.CountSource);
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

public sealed class TunnelHTTP2RouteOptions {
}