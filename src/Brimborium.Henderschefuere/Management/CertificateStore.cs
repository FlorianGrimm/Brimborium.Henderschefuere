using System.Security.Cryptography.X509Certificates;

namespace Brimborium.Henderschefuere.Management;


public interface ICertificateStore {
    X509Certificate? GetCertificate(string name);
}

public class CertificateStore : ICertificateStore {
    private readonly List<ICertificateStore> _CertificateServices;
    private readonly ILogger _Logger;

    public CertificateStore(
        IEnumerable<ICertificateStore> listCertificateService,
        ILogger<CertificateStore> logger
        ) {
        this._CertificateServices = listCertificateService.ToList();
        this._Logger = logger;
    }

    public List<ICertificateStore> CertificateServices => this._CertificateServices;

    public X509Certificate? GetCertificate(string name) {
        foreach (var certificateService in _CertificateServices) {
            var certificate = certificateService.GetCertificate(name);
            if (certificate is not null) { return certificate; }
        }
        return null;
    }

    /*
    
    private ConcurrentDictionary<string, X509Certificate> _certificateByFilename = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, X509Certificate> _certificateBySerialNumber = new(StringComparer.OrdinalIgnoreCase);

    public X509Certificate? GetCertificateByFileName(string fileName, string? password) {
        try {
            X509Certificate.CreateFromCertFile
            var result = new X509Certificate(fileName, password, X509KeyStorageFlags.PersistKeySe);
            _certificateBySerialNumber[result.GetSerialNumberString()] = result;
            return result;
        } catch (System.Exception error){
            this._Logger.LogError(error, "GetCertificate {fileName}", fileName);
            return null;
        }
    }
    */
}
