namespace Microsoft.Extensions.DependencyInjection;

public static class WebHostBuilderExtensions {
    public static IReverseProxyBuilder UseTunnelTransport(
        this IReverseProxyBuilder builder,
        WebApplicationBuilder webApplicationBuilder,
        Action<TransportTunnelHttp2Options>? configureTunnelHttp2 = null,
        Action<TransportTunnelWebSocketOptions>? configureTunnelWebSocket = null
        ) {
        builder.Services.TryAddSingleton<OptionalCertificateStoreFactory>();
        builder.Services.TryAddSingleton<OptionalCertificateStore>();
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
