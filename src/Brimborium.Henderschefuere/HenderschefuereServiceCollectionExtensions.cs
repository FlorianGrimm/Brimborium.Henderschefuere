using Brimborium.Henderschefuere.Model;
using Brimborium.Henderschefuere.Tunnel;

namespace Microsoft.Extensions.DependencyInjection;

public static class HenderschefuereServiceCollectionExtensions {

    public static HenderschefuereBuilder AddHenderschefuere(
        this IServiceCollection services
        ) {
        HenderschefuereBuilder? builder = services
            .LastOrDefault((serviceDescriptor) => typeof(HenderschefuereBuilder).Equals(serviceDescriptor.ServiceType))?
            .ImplementationInstance as HenderschefuereBuilder;
        if (builder is null) {
            builder = new(services);
            services.AddSingleton<HenderschefuereBuilder>(builder);
            services.AddSingleton<HfConfiguration>(builder.Configuration);
        }
        services.TryAddSingleton<HfConfigurationManager>();
        services.TryAddSingleton<HfEndpointDataSource>();
        services.TryAddSingleton<ITunnelConnectionListenerFactory, TunnelWebSocketConnectionContext.Factory >();
        services.TryAddSingleton<ITunnelConnectionListenerFactory, TunnelHttp2ConnectionContext.Factory >();
        services.TryAddSingleton<HfModelManager>();
        return builder;
    }
}