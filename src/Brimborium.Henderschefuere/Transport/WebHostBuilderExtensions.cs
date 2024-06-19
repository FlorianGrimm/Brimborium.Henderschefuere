namespace Brimborium.Henderschefuere.Transport;

#warning WEICHEI??

public static class WebHostBuilderExtensions {
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
}
