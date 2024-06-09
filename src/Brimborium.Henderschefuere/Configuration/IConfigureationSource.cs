namespace Brimborium.Henderschefuere.Configuration;

public interface IHfConfigurationSource {
    CancellationChangeToken ConfigurationChanged { get; }

    HfRootConfiguration GetRootConfiguration();
}

public sealed class HfConfigurationSource : IHfConfigurationSource, IDisposable {
    private readonly IConfiguration _Configuration;
    private IDisposable? _ConfigurationRegistation;
    private CancellationTokenSource _CtsConfigurationChanged;

    public CancellationChangeToken _ConfigurationChanged;
    public CancellationChangeToken ConfigurationChanged => this._ConfigurationChanged;

    private HfRootConfiguration? _RootConfiguration;
    public HfRootConfiguration? RootConfiguration => this._RootConfiguration;

    public HfConfigurationSource(IConfiguration configuration) {
        this._CtsConfigurationChanged = new();
        this._ConfigurationChanged = new CancellationChangeToken(this._CtsConfigurationChanged.Token);
        this._Configuration = configuration;
        this.WireConfiguration();
        this.LoadConfiguration();
    }
    private void WireConfiguration() {
        this._ConfigurationRegistation?.Dispose();
        this._ConfigurationRegistation = ChangeToken.OnChange(
            () => {
                return this._Configuration.GetReloadToken();
            },
            () => {
                System.Console.WriteLine("HfConfigurationSource ConfigurationChanged");
                this.LoadConfiguration();
            });
    }

    private HfRootConfiguration LoadConfiguration() {
        HfRootConfiguration result = this.BindHfRootConfiguration(this._Configuration);
        lock (this) {
            var oldCtsConfigurationChanged = this._CtsConfigurationChanged;
            this._RootConfiguration = result;
            this._CtsConfigurationChanged = new();
            this._ConfigurationChanged = new CancellationChangeToken(this._CtsConfigurationChanged.Token);
            System.Threading.Interlocked.MemoryBarrier();
            oldCtsConfigurationChanged?.Cancel();
        }
        return result;
    }

    public HfRootConfiguration GetRootConfiguration() {
        return this._RootConfiguration ??= this.LoadConfiguration();
    }

    public HfRootConfiguration BindHfRootConfiguration(IConfiguration configuration) {
        var result = new HfRootConfiguration();
        {
            var section = configuration.GetSection("Tunnels");
            foreach (var child in section.GetChildren()) {
                var key = child.Key;
                var value = new HfTunnelConfiguration();
                value.Id = key;
                value.Url = child[nameof(value.Url)];
                value.RemoteTunnelId = child[nameof(value.RemoteTunnelId)];
                value.Transport = this.ParseHfTunnelTransport(child[nameof(value.Transport)]);
                value.Authentication = this.BindHfTunnelAuthenticationConfiguration(configuration.GetSection(nameof(value.Authentication)));
                result.Tunnels[key] = value;
            }
        }
        {
            var section = configuration.GetSection("Clusters");
            foreach (var child in section.GetChildren()) {
                var key = child.Key;
                var value = new HfClusterConfiguration();
                value.Id = key;
                result.Clusters[key] = value;
                value.Transport = this.ParseHfTunnelTransport(child[nameof(value.Transport)]);
                var sectionDestinations = section.GetSection(nameof(value.Destinations));
                foreach (var childDestination in sectionDestinations.GetChildren()) {
                    var childKey = childDestination.Key;
                    var childValue = new HfClusterDestinationConfiguration();
                    childValue.Id = childKey;
                    childValue.Address = childDestination[nameof(childValue.Address)];
                    value.Destinations[childKey] = childValue;
                }
            }
        }
        {
            var section = configuration.GetSection("Routes");
            foreach (var child in section.GetChildren()) {
                var key = child.Key;
                var value = new HfRouteConfiguration();
                value.Id = key;
                value.ClusterId = child[nameof(value.ClusterId)];
                value.Match = this.BindHfRouteMatchConfiguration(section.GetSection(nameof(value.Match)));
                result.Routes[key] = value;
            }
        }
        return result;
    }

    private HfTransport? ParseHfTunnelTransport(string? value) {
        if (string.IsNullOrEmpty(value)) {
            return null;
        } else if (string.Equals(value, nameof(HfTransport.ReverseProxy), StringComparison.OrdinalIgnoreCase)) {
            return HfTransport.ReverseProxy;
        } else if (string.Equals(value, nameof(HfTransport.TunnelHTTP2), StringComparison.OrdinalIgnoreCase)) {
            return HfTransport.TunnelHTTP2;
        } else if (string.Equals(value, nameof(HfTransport.TunnelWebSocket), StringComparison.OrdinalIgnoreCase)) {
            return HfTransport.TunnelWebSocket;
        } else {
            return null;
        }
    }

    private HfTunnelAuthenticationConfiguration? BindHfTunnelAuthenticationConfiguration(IConfigurationSection configuration) {
        return null;
    }

    private HfRouteMatchConfiguration? BindHfRouteMatchConfiguration(IConfigurationSection configuration) {
        if (configuration.Exists()) {
            var result = new HfRouteMatchConfiguration();
            result.Id = configuration[configuration.Key];
            result.Path = configuration[nameof(result.Path)];
            return result;
        } else {
            return null;
        }
    }


    public void Dispose() {
        using (var ctsConfigurationChanged = this._CtsConfigurationChanged) {
            using (var configurationRegistation = this._ConfigurationRegistation) {
                this._ConfigurationRegistation = null!;
                this._CtsConfigurationChanged = null!;
            }
        }
    }
}
