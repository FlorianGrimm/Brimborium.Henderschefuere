// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Brimborium.Henderschefuere.Routing;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for <see cref="IEndpointRouteBuilder"/>
/// used to add Reverse Proxy to the ASP .NET Core request pipeline.
/// </summary>
public static class ReverseProxyIEndpointRouteBuilderExtensions {
    /// <summary>
    /// Adds Reverse Proxy routes to the route table using the default processing pipeline.
    /// </summary>
    public static ReverseProxyConventionBuilder MapReverseProxy(this IEndpointRouteBuilder endpoints) {
        return endpoints.MapReverseProxy(app => {
            app.UseSessionAffinity();
            app.UseLoadBalancing();
            app.UsePassiveHealthChecks();
        });
    }

    /// <summary>
    /// Adds Reverse Proxy routes to the route table with the customized processing pipeline. The pipeline includes
    /// by default the initialization step and the final proxy step, but not LoadBalancingMiddleware or other intermediate components.
    /// </summary>
    public static ReverseProxyConventionBuilder MapReverseProxy(this IEndpointRouteBuilder endpoints, Action<IReverseProxyApplicationBuilder> configureApp) {
        if (endpoints is null) {
            throw new ArgumentNullException(nameof(endpoints));
        }
        if (configureApp is null) {
            throw new ArgumentNullException(nameof(configureApp));
        }

        var proxyAppBuilder = new ReverseProxyApplicationBuilder(endpoints.CreateApplicationBuilder());
        proxyAppBuilder.UseMiddleware<ProxyPipelineInitializerMiddleware>();
        configureApp(proxyAppBuilder);
        proxyAppBuilder.UseMiddleware<LimitsMiddleware>();
        proxyAppBuilder.UseMiddleware<ForwarderMiddleware>();
        var app = proxyAppBuilder.Build();

        var proxyEndpointFactory = endpoints.ServiceProvider.GetRequiredService<ProxyEndpointFactory>();
        proxyEndpointFactory.SetProxyPipeline(app);

        var proxyConfigManager = GetOrCreateDataSource(endpoints);

        var tunnelClusters = proxyConfigManager.GetTransportTunnelClusters();
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


        return proxyConfigManager.DefaultBuilder;
    }

    private static ProxyConfigManager GetOrCreateDataSource(IEndpointRouteBuilder endpoints) {
        var proxyConfigManager = endpoints.DataSources.OfType<ProxyConfigManager>().FirstOrDefault();
        if (proxyConfigManager is null) {
            proxyConfigManager = endpoints.ServiceProvider.GetRequiredService<ProxyConfigManager>();
            endpoints.DataSources.Add(proxyConfigManager);

            // Config validation is async but startup is sync. We want this to block so that A) any validation errors can prevent
            // the app from starting, and B) so that all the config is ready before the server starts accepting requests.
            // Reloads will be async.
            proxyConfigManager.InitialLoadAsync().GetAwaiter().GetResult();
        }

        return proxyConfigManager;
    }
}
