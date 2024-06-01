namespace Brimborium.Henderschefuere.Configuration;

public interface IConfigurationSource {
    CancellationChangeToken ConfigurationChanged { get; }
}

public class ConfigurationSource: IConfigurationSource, IDisposable {
    private readonly IConfiguration _Configuration;
    private IDisposable _ConfigurationRegistation;
    private CancellationTokenSource _CtsConfigurationChanged = new();
    public CancellationChangeToken ConfigurationChanged { get; }
    public HfRootConfiguration RootConfiguration { get; private set; } = new();

    public ConfigurationSource(IConfiguration configuration)
    {
        this.ConfigurationChanged = new CancellationChangeToken(this._CtsConfigurationChanged.Token);
        this._Configuration = configuration;
        this._ConfigurationRegistation = ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => this.OnConfigurationReload());
        this.OnConfigurationReload();
    }

    private void OnConfigurationReload() {
        this.RootConfiguration = this.Bind(this._Configuration);

        if (this._CtsConfigurationChanged.IsCancellationRequested) {
            // do nothing
        } else {
            this._CtsConfigurationChanged.Cancel();
        }
    }

    public HfRootConfiguration GetRootConfiguration() {
        if (this._CtsConfigurationChanged.IsCancellationRequested) { 
            this._CtsConfigurationChanged.TryReset();
        }
        return this.RootConfiguration;
    }

    public HfRootConfiguration Bind(IConfiguration configuration) {
        var result = new HfRootConfiguration();
        configuration.GetSection("Tunnels");
        configuration.GetSection("Routes");
        configuration.GetSection("Clusters");
        return result;
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
