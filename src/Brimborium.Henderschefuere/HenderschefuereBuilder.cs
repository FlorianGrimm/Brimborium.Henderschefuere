using Brimborium.Henderschefuere.Tunnel;

using Microsoft.AspNetCore.Hosting;

namespace Brimborium.Henderschefuere;

public sealed class HenderschefuereBuilder(IServiceCollection services) {
    public IServiceCollection Services { get; } = services;
    public WebApplicationBuilder? Builder { get; private set; }

    public HfConfiguration Configuration { get; } = new();


    public HenderschefuereBuilder LoadFromConfigurationDefault(IConfigurationRoot configurationRoot) {
        return this.LoadFromConfiguration(configurationRoot.GetSection("Henderschefuere"));
    }

    public HenderschefuereBuilder LoadFromConfiguration(IConfiguration configuration) {
        this.Configuration.ListHfConfigurationSource.Add(new HfConfigurationSource(configuration));
        return this;
    }

    public HenderschefuereBuilder EnableTunnel(WebApplicationBuilder builder) {
        this.Builder = builder;

        builder.WebHost.ConfigureKestrel((kestrelOptions) => {
            var configurationManager = kestrelOptions.ApplicationServices.GetRequiredService<HfConfigurationManager>();
            var snapshot = configurationManager.GetSnapshot();
            foreach (var tunnel in snapshot.Tunnels.Values) {
                if (string.IsNullOrEmpty(tunnel.Url)) { continue; }
                
                if (tunnel.Transport == HfTransport.TunnelHTTP2) {
                    kestrelOptions.Listen(new Http2UriEndPoint(new(tunnel.Url), tunnel));
                    continue;
                }

                if (tunnel.Transport == HfTransport.TunnelWebSocket) {
                    kestrelOptions.Listen(new WebSocketUriEndPoint(new(tunnel.Url), tunnel));
                    continue;
                }
            }
        });
        return this;
    }
}

public sealed class HfConfiguration {
    public readonly List<IHfConfigurationSource> ListHfConfigurationSource = new();
}