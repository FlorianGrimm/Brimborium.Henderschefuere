using System.Security.Cryptography.X509Certificates;

namespace Brimborium.Henderschefuere.Management;

public class CertificateStoreSystemFactory : ICertificateStoreFactory {
    private readonly ILogger _logger;

    public CertificateStoreSystemFactory(
        ILogger<CertificateStoreSystem> logger
        ) {
        this._logger = logger;
    }

    public ICertificateStore? CreateCertificateStore(CertificateStoreOptions options) {
        if (!string.Equals("System", options.Kind, StringComparison.OrdinalIgnoreCase)) { return null; }

        return new CertificateStoreSystem(options, _logger);
    }
}

public class CertificateStoreSystem : ICertificateStore {
    private readonly ILogger _logger;
    private readonly X509Store _store;

    public CertificateStoreSystem(
        CertificateStoreOptions options,
        ILogger logger
        ) {
        _logger = logger;
        _store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    }

    public X509Certificate? GetCertificate(string name) {
        var certificateCollection = _store.Certificates;
        {
            var result = certificateCollection.Find(X509FindType.FindBySerialNumber, name, true);
            if (result is { Count: > 0 }) {
                return result[0];
            }
        }
        {
            var result = certificateCollection.Find(X509FindType.FindBySubjectName, name, true);
            if (result is { Count: > 0 }) {
                return result[0];
            }
        }

        return null;
    }
}
