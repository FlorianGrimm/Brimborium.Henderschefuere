namespace Microsoft.Extensions.DependencyInjection;

public static class TunnelExensions {
    public static IServiceCollection AddTunnelServices(this IServiceCollection services) {
        services.TryAddSingleton<TunnelConnectionChannelManager>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IClusterChangeListener>(sp => sp.GetRequiredService<TunnelConnectionChannelManager>()));
        services.TryAddSingleton<TransportHttpClientFactorySelector>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelHTTP2HttpClientFactory>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelWebSocketHttpClientFactory>());
        services.TryAddSingleton<OptionalCertificateStoreFactory>();
        services.TryAddSingleton<OptionalCertificateStore>();
        services.TryAddSingleton<TunnelHTTP2Route>();
        services.TryAddSingleton<TunnelWebSocketRoute>();
        return services;
    }

    public static IReverseProxyBuilder AddTunnelServices(
        this IReverseProxyBuilder builder) {
        builder.Services.AddTunnelServices();
        return builder;
    }

#if false
    internal static void MapTunnels(this IEndpointRouteBuilder endpoints, IEnumerable<ClusterState> tunnelClusters) {
        foreach (var cluster in tunnelClusters) {
            var transport = cluster.Model.Config.Transport;
            if (transport == TransportMode.TunnelHTTP2) {
                endpoints.MapHttp2Tunnel(cluster);
                continue;
            }
            if (transport == TransportMode.TunnelWebSocket) {
                endpoints.MapWebSocketTunnel(cluster);
                continue;
            }
        }
        return;
    }

    internal static IEndpointConventionBuilder MapHttp2Tunnel(this IEndpointRouteBuilder routes, ClusterState clusterState) {
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

    internal static IEndpointConventionBuilder MapWebSocketTunnel(this IEndpointRouteBuilder routes, ClusterState clusterState) {
        var cfg = clusterState.Model.Config;
        var tunnelClientFactory = routes.ServiceProvider.GetRequiredService<TunnelConnectionChannelManager>();
        if (!(tunnelClientFactory.RegisterConnectionChannel(cfg.ClusterId) is { } tunnelConnectionChannels)) {
            throw new Exception("ClusterId already exists.");
        }
        var lifetime = routes.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
        var route = new TunnelWebSocketRoute(tunnelConnectionChannels, clusterState, lifetime);
        return route.Map(routes);
    }
#else
    internal static void MapTunnels(this IEndpointRouteBuilder endpoints, IEnumerable<ClusterState> tunnelClusters) {
        if (!tunnelClusters.Any()) { return; }

        var tunnelHTTP2Route = endpoints.ServiceProvider.GetRequiredService<TunnelHTTP2Route>();
        tunnelHTTP2Route.Map(endpoints);
        tunnelHTTP2Route.Register(tunnelClusters);

        var tunnelWebSocketRoute = endpoints.ServiceProvider.GetRequiredService<TunnelWebSocketRoute>();
        tunnelWebSocketRoute.Map(endpoints);
        tunnelWebSocketRoute.Register(tunnelClusters);
    }
#endif
}
