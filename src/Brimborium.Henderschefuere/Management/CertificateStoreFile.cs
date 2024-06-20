using System.Security.Cryptography.X509Certificates;

namespace Brimborium.Henderschefuere.Management;

public class CertificateStoreFileFactory : ICertificateStoreFactory {
    private readonly ILogger _logger;

    public CertificateStoreFileFactory(
        ILogger<CertificateStoreFile> logger
        ) {
        this._logger = logger;
    }

    public ICertificateStore? CreateCertificateStore(CertificateStoreOptions options) {
        if (!string.Equals("File", options.Kind, StringComparison.OrdinalIgnoreCase)) { return null; }

        return new CertificateStoreFile(options, _logger);
    }
}

public sealed class CertificateStoreFile : ICertificateStore {
    private readonly string? _location;
    private readonly string? _password;
    private readonly ILogger _logger;

    // TODO: think about FileSystemWatcher and chache
    // private readonly FileSystemWatcher _watcher;

    public CertificateStoreFile(
        CertificateStoreOptions options,
        ILogger logger
        ) {
        _location = options.Location;
        _password = options.Password;
        _logger = logger;
    }

    public X509Certificate? GetCertificate(string name) {
        if (string.IsNullOrEmpty(_location)) { return null; }

        var certificateCollection = new X509Certificate2Collection();
        try {
            var certificate = new X509Certificate2(_location, _password, X509KeyStorageFlags.PersistKeySet);
            certificateCollection.Add(certificate);
        } catch {
            return null;
        }
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