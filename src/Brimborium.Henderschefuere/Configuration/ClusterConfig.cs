// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Configuration;

/// <summary>
/// A cluster is a group of equivalent endpoints and associated policies.
/// </summary>
public sealed record ClusterConfig {
    /// <summary>
    /// The Id for this cluster. This needs to be globally unique.
    /// This field is required.
    /// </summary>
    public string ClusterId { get; init; } = default!;

    /// <summary>
    /// Load balancing policy.
    /// </summary>
    public string? LoadBalancingPolicy { get; init; }

    /// <summary>
    /// Session affinity config.
    /// </summary>
    public SessionAffinityConfig? SessionAffinity { get; init; }

    /// <summary>
    /// Health checking config.
    /// </summary>
    public HealthCheckConfig? HealthCheck { get; init; }

    /// <summary>
    /// Config for the HTTP client that is used to call destinations in this cluster.
    /// </summary>
    public HttpClientConfig? HttpClient { get; init; }

    /// <summary>
    /// Config for outgoing HTTP requests.
    /// </summary>
    public ForwarderRequestConfig? HttpRequest { get; init; }

    /// <summary>
    /// The set of destinations associated with this cluster.
    /// </summary>
    public IReadOnlyDictionary<string, DestinationConfig>? Destinations { get; init; }

    /// <summary>
    /// Arbitrary key-value pairs that further describe this cluster.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public TransportMode Transport { get; init; }

    public TransportAuthentication Authentication { get; init; } = new();

    public bool IsTunnelTransport => Transport == TransportMode.TunnelHTTP2 || Transport == TransportMode.TunnelWebSocket;

    public bool Equals(ClusterConfig? other) {
        if (other is null) {
            return false;
        }

        return EqualsExcludingDestinations(other)
            && CollectionEqualityHelper.Equals(Destinations, other.Destinations);
    }

    internal bool EqualsExcludingDestinations(ClusterConfig other) {
        if (other is null) {
            return false;
        }

        return string.Equals(ClusterId, other.ClusterId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(LoadBalancingPolicy, other.LoadBalancingPolicy, StringComparison.OrdinalIgnoreCase)
            // CS0252 warning only shows up in VS https://github.com/dotnet/roslyn/issues/49302
            && SessionAffinity == other.SessionAffinity
            && HealthCheck == other.HealthCheck
            && HttpClient == other.HttpClient
            && HttpRequest == other.HttpRequest
            && Transport == other.Transport
            && Authentication == other.Authentication
            && CaseSensitiveEqualHelper.Equals(Metadata, other.Metadata);
    }

    public override int GetHashCode() {
        HashCode hashCode = new HashCode();
        hashCode.Add(ClusterId, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(LoadBalancingPolicy, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(SessionAffinity);
        hashCode.Add(HealthCheck);
        hashCode.Add(HttpClient);
        hashCode.Add(HttpRequest);
        hashCode.Add(Transport);
        hashCode.Add(Authentication);
        hashCode.Add(CollectionEqualityHelper.GetHashCode(Destinations));
        hashCode.Add(CaseSensitiveEqualHelper.GetHashCode(Metadata));
        return hashCode.ToHashCode();
    }
}
