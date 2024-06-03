using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

public static class HenderschefuereBuilderExtensions {
    //public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern) where TConnectionHandler : ConnectionHandler;

    public static void MapHenderschefuere(this IEndpointRouteBuilder endpoints) {
        var henderschefuereBuilder = endpoints.ServiceProvider.GetRequiredService<HenderschefuereBuilder>();
        var proxyConfigManager = endpoints.ServiceProvider.GetRequiredService<HfConfigurationManager>();
        proxyConfigManager.SetHenderschefuereConfiguration(henderschefuereBuilder.Configuration);
        var dataSource = endpoints.DataSources.OfType<HfEndpointDataSource>().FirstOrDefault();
        if (dataSource is null) {
            dataSource = endpoints.ServiceProvider.GetRequiredService<HfEndpointDataSource>();
            endpoints.DataSources.Add(dataSource);
        }
        // var proxyAppBuilder = new ReverseProxyApplicationBuilder(endpoints.CreateApplicationBuilder());
    }
}
