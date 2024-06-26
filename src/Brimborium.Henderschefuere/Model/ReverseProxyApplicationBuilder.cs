// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Model;

public sealed class ReverseProxyApplicationBuilder : IReverseProxyApplicationBuilder {
    private readonly IApplicationBuilder _applicationBuilder;

    public ReverseProxyApplicationBuilder(IApplicationBuilder applicationBuilder) {
        _applicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
    }

    public IServiceProvider ApplicationServices {
        get => _applicationBuilder.ApplicationServices;
        set => _applicationBuilder.ApplicationServices = value;
    }

    public IFeatureCollection ServerFeatures => _applicationBuilder.ServerFeatures;

    public IDictionary<string, object?> Properties => _applicationBuilder.Properties;

    public RequestDelegate Build() => _applicationBuilder.Build();

    public IApplicationBuilder New() => _applicationBuilder.New();

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        => _applicationBuilder.Use(middleware);
}
