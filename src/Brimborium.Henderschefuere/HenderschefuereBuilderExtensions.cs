namespace Microsoft.AspNetCore.Builder;

public static class HenderschefuereBuilderExtensions {
    //public static ConnectionEndpointRouteBuilder MapConnectionHandler<TConnectionHandler>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern) where TConnectionHandler : ConnectionHandler;

    public static void MapHenderschefuere(this IEndpointRouteBuilder endpoints) {
        var sp=endpoints.ServiceProvider;
        // endpoints.DataSources.Add(new HenderschefuereDataSource());
    }
}
