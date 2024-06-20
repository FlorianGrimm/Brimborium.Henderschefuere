namespace Microsoft.Extensions.DependencyInjection;

public static class TunnelExensions {
    public static IServiceCollection AddTunnelServices(this IServiceCollection services) {
        services.TryAddSingleton<TunnelConnectionChannelManager>();
        services.TryAddSingleton<TransportHttpClientFactorySelector>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelHTTP2HttpClientFactory>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelWebSocketHttpClientFactory>());
        services.TryAddSingleton<OptionalCertificateStoreFactory>();
        services.TryAddSingleton<OptionalCertificateStore>();
        return services;
    }

    public static IReverseProxyBuilder AddTunnelServices(
        this IReverseProxyBuilder builder) {
        builder.Services.AddTunnelServices();
        return builder;
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
}
