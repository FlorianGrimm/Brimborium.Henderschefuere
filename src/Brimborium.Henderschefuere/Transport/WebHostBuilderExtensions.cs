namespace Brimborium.Henderschefuere.Transport;


public static class WebHostBuilderExtensions {
    public static IServiceCollection AddTunnelServices(this IServiceCollection services) {
        services.TryAddSingleton<TunnelConnectionChannelManager>();
        services.TryAddSingleton<TransportHttpClientFactorySelector>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelHTTP2HttpClientFactory>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITransportHttpClientFactorySelector, TunnelWebSocketHttpClientFactory>());
        return services;
    }

    // frontend
    public static IReverseProxyBuilder AddTunnelServices(
        this IReverseProxyBuilder builder) {
        builder.Services.AddTunnelServices();
        return builder;
    }

#if WEICHEI
    public static IWebHostBuilder UseTunnelTransportHttp2(this IWebHostBuilder hostBuilder, UriEndPointHttp2 endPoint, Action<TransportTunnelHttp2Options>? configure = null) {
        ArgumentNullException.ThrowIfNull(endPoint);

        hostBuilder.ConfigureKestrel(options => {
            options.Listen(endPoint);
        });

        return hostBuilder.ConfigureServices(services => {
            services.AddSingleton<IConnectionListenerFactory, TransportTunnelHttp2ConnectionListenerFactory>();

            if (configure is not null) {
                services.Configure(configure);
            }
        });
    }

    public static IWebHostBuilder UseTunnelTransportWebSocket(this IWebHostBuilder hostBuilder, UriWebSocketEndPoint endPoint, Action<TransportTunnelWebSocketOptions>? configure = null) {
        ArgumentNullException.ThrowIfNull(endPoint);

        hostBuilder.ConfigureKestrel(options => {
            options.Listen(endPoint);
        });

        return hostBuilder.ConfigureServices(services => {
            services.AddSingleton<IConnectionListenerFactory, TransportTunnelWebSocketConnectionListenerFactory>();

            if (configure is not null) {
                services.Configure(configure);
            }
        });
    }
#endif
}
