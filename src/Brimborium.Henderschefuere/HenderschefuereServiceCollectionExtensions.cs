namespace Microsoft.Extensions.DependencyInjection;

public static class HenderschefuereServiceCollectionExtensions {

    public static HenderschefuereBuilder AddHenderschefuere(this IServiceCollection services
        //, Action<SwaggerGenOptions> setupAction = null
        ) {
        HenderschefuereBuilder? builder = services
            .LastOrDefault((serviceDescriptor) => typeof(HenderschefuereBuilder).Equals(serviceDescriptor.ServiceType))?
            .ImplementationInstance as HenderschefuereBuilder;
        if (builder is null) {
            builder = new(services);
            services.AddSingleton<HenderschefuereBuilder>(builder);
        }
        services.TryAddSingleton<ProxyConfigManager>();
        return builder;
    }
}