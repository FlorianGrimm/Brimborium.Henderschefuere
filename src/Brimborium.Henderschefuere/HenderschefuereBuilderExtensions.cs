namespace Microsoft.AspNetCore.Builder;

public static class HenderschefuereBuilderExtensions {
    //public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern) where TConnectionHandler : ConnectionHandler;

    public static void MapHenderschefuere(this IEndpointRouteBuilder endpoints) {
        var serviceProvider = endpoints.ServiceProvider;
        var endpointDataSource = endpoints.DataSources.OfType<HfEndpointDataSource>().FirstOrDefault();
        if (endpointDataSource is null) {
            var henderschefuereBuilder = serviceProvider.GetRequiredService<HenderschefuereBuilder>();
            var configurationManager = serviceProvider.GetRequiredService<HfConfigurationManager>();
            configurationManager.SetHenderschefuereConfiguration(henderschefuereBuilder.Configuration);
            var modelManager = serviceProvider.GetRequiredService<HfModelManager>();
            modelManager.SetConfigurationManager(configurationManager);

            var proxyAppBuilder = new HfApplicationBuilder(endpoints.CreateApplicationBuilder());
            var app = proxyAppBuilder.Build();

            /*
            var proxyAppBuilder = new ReverseProxyApplicationBuilder(endpoints.CreateApplicationBuilder());
            proxyAppBuilder.UseMiddleware<ProxyPipelineInitializerMiddleware>();
            configureApp(proxyAppBuilder);
            proxyAppBuilder.UseMiddleware<LimitsMiddleware>();
            proxyAppBuilder.UseMiddleware<ForwarderMiddleware>();
            var app = proxyAppBuilder.Build();

            var proxyEndpointFactory = endpoints.ServiceProvider.GetRequiredService<ProxyEndpointFactory>();
            proxyEndpointFactory.SetProxyPipeline(app);         
             */



            endpointDataSource = serviceProvider.GetRequiredService<HfEndpointDataSource>();
            modelManager.SetEndpointDataSource(endpointDataSource, app);
            endpoints.DataSources.Add(endpointDataSource);
        }


    }
}
