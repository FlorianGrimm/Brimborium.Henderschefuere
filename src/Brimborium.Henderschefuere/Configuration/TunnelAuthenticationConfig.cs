// Licensed under the MIT License.

using System.Security.Cryptography.X509Certificates;

namespace Brimborium.Henderschefuere.Configuration;

public record TunnelAuthenticationConfig {
    public string? ClientCertificate { get; init; }

    // for in-memory configuration
    public X509CertificateCollection? ClientCertifiacteCollection { get; init; }
}
