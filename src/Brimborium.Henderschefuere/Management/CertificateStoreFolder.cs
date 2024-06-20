using System.Security.Cryptography.X509Certificates;

namespace Brimborium.Henderschefuere.Management;

public class CertificateStoreFolderFactory : ICertificateStoreFactory {
    private readonly ILogger _logger;

    public CertificateStoreFolderFactory(
        ILogger<CertificateStoreFolder> logger
        ) {
        this._logger = logger;
    }

    public ICertificateStore? CreateCertificateStore(CertificateStoreOptions factoryOptions) {
        if (!string.Equals("Folder", factoryOptions.Kind, StringComparison.OrdinalIgnoreCase)) { return null; }

        return new CertificateStoreFolder(factoryOptions, _logger);
    }
}

public sealed class CertificateStoreFolder : ICertificateStore {
    private readonly string? _location;
    private readonly string? _password;
    private readonly ILogger _logger;
    private X509Certificate2Collection? _certificateCollection;

    public CertificateStoreFolder(
        CertificateStoreOptions factoryOptions,
        ILogger logger
        ) {
        _location = factoryOptions.Location;
        _password = factoryOptions.Password;
        _logger = logger;
    }

    public X509Certificate? GetCertificate(string name) {
        if (string.IsNullOrEmpty(_location)) { return null; }

        var certificateCollection = new X509Certificate2Collection();
        System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(_location);
        var listFiles = directoryInfo.EnumerateFiles("*.*");
        foreach (var file in listFiles) {
            X509Certificate2 certificate;
            try {
                certificate = new X509Certificate2(file.FullName, _password, X509KeyStorageFlags.PersistKeySet);
                certificateCollection.Add(certificate);
            } catch {
                continue;
            }
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