namespace Brimborium.Henderschefuere;

public sealed class HenderschefuereBuilder(IServiceCollection services) {
    public IServiceCollection Services { get; } = services;

    public readonly List<IConfiguration> ListConfigurations = new();

    public HenderschefuereBuilder LoadFromConfigurationDefault(IConfigurationRoot configurationRoot) {
        return this.LoadFromConfiguration(configurationRoot.GetSection("Henderschefuere"));
    }
    public HenderschefuereBuilder LoadFromConfiguration(IConfiguration configuration) {
        this.ListConfigurations.Add(configuration);
        return this;
    }
}
