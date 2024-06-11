
namespace Microsoft.AspNetCore.Builder;

internal class HfApplicationBuilder {
    private IApplicationBuilder _ApplicationBuilder;

    public HfApplicationBuilder(IApplicationBuilder applicationBuilder) {
        ArgumentNullException.ThrowIfNull(applicationBuilder, nameof(applicationBuilder));
        this._ApplicationBuilder = applicationBuilder;
    }


    public IServiceProvider ApplicationServices {
        get => _ApplicationBuilder.ApplicationServices;
        set => _ApplicationBuilder.ApplicationServices = value;
    }

    public IFeatureCollection ServerFeatures => _ApplicationBuilder.ServerFeatures;

    public IDictionary<string, object?> Properties => _ApplicationBuilder.Properties;

    public RequestDelegate Build() => _ApplicationBuilder.Build();

    public IApplicationBuilder New() => _ApplicationBuilder.New();

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        => _ApplicationBuilder.Use(middleware);
}