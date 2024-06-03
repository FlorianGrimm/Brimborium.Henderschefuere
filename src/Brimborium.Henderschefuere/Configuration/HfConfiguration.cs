namespace Brimborium.Henderschefuere.Configuration;

public sealed class HfRootConfiguration {
    public Dictionary<string, HfTunnelConfiguration> Tunnels { get; set; } = new();
    public Dictionary<string, HfRouteConfiguration> Routes { get; set; } = new();
    public Dictionary<string, HfClusterConfiguration> Clusters { get; set; } = new();
}

public enum HfTransport { None, ReverseProxy, TunnelHTTP2, TunnelWebSocket }

public sealed class HfTunnelConfiguration {
    public string? Id { get; set; }
    public string? Url { get; set; }
    public string? RemoteTunnelId { get; set; }
    public HfTransport? Transport { get; set; }
    public HfTunnelAuthenticationConfiguration? Authentication { get; set; }
}

public sealed class HfTunnelAuthenticationConfiguration {
}

public sealed class HfRouteConfiguration {
    public string? Id { get; set; }
    public string? ClusterId { get; set; }
    public HfRouteMatchConfiguration? Match { get; set; }
}
public sealed class HfRouteMatchConfiguration {
    public string? Id { get; set; }
    public string? Path { get; set; }
}

public sealed class HfClusterConfiguration {
    public string? Id { get; set; }
    public HfTransport? Transport { get; set; }
    public Dictionary<string, HfClusterDestinationConfiguration> Destinations { get; } = new ();
}

public sealed class HfClusterDestinationConfiguration {
    public string? Id { get; set; }
    public string? Address { get; set; }
}