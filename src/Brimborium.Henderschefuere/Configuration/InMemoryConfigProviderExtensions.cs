// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Extensions.DependencyInjection;

public static class InMemoryConfigProviderExtensions {
    /// <summary>
    /// Adds an InMemoryConfigProvider
    /// </summary>
    public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) {
        builder.Services.AddSingleton(new InMemoryConfigProvider([], routes, clusters));
        builder.Services.AddSingleton<IProxyConfigProvider>(s => s.GetRequiredService<InMemoryConfigProvider>());
        return builder;
    }

    /// <summary>
    /// Adds an InMemoryConfigProvider
    /// </summary>
    public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IReadOnlyList<TunnelConfig> tunnels, IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) {
        builder.Services.AddSingleton(new InMemoryConfigProvider(tunnels, routes, clusters));
        builder.Services.AddSingleton<IProxyConfigProvider>(s => s.GetRequiredService<InMemoryConfigProvider>());
        return builder;
    }
}
