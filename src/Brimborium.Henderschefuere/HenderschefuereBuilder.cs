namespace Brimborium.Henderschefuere;

public sealed class HenderschefuereBuilder(IServiceCollection services) {
    public IServiceCollection Services { get; } = services;

    public HfConfiguration Configuration { get; } = new();

    public HenderschefuereBuilder LoadFromConfigurationDefault(IConfigurationRoot configurationRoot) {
        return this.LoadFromConfiguration(configurationRoot.GetSection("Henderschefuere"));
    }
    public HenderschefuereBuilder LoadFromConfiguration(IConfiguration configuration) {
        this.Configuration.ListHfConfigurationSource.Add(new HfConfigurationSource(configuration));
        return this;
    }
}

public sealed class HfConfiguration {
    public readonly List<IHfConfigurationSource> ListHfConfigurationSource = new();
}