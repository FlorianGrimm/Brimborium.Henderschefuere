namespace Brimborium.Henderschefuere.Configuration;

public class HfRootConfiguration {
    public Dictionary<string, HfTunnelConfiguration> Tunnels { get; set; } = new();
    public Dictionary<string, HfRouteConfiguration> Routes { get; set; } = new();
    public Dictionary<string, HfClusterConfiguration> Clusters { get; set; } = new();
}

public class HfTunnelConfiguration {
}

public class HfRouteConfiguration {
    
}

public class HfClusterConfiguration {
}