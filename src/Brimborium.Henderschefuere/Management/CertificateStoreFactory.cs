namespace Brimborium.Henderschefuere.Management;

public record CertificateStoreOptions {
    public string? Kind { get; set; }
    public string? Location { get; set; }
    public string? Password { get; set; }
}

public interface ICertificateStoreFactory {
    ICertificateStore? CreateCertificateStore(CertificateStoreOptions factoryOptions);
}

public class CertificateStoreFactory : ICertificateStoreFactory {
    private readonly CertificateStore _certificateStore;
    private readonly IEnumerable<ICertificateStoreFactory> _listCertificateServiceFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public CertificateStoreFactory(
        CertificateStore certificateStore,
        IEnumerable<ICertificateStoreFactory> listCertificateServiceFactory,
        ILoggerFactory loggerFactory,
        ILogger<CertificateStoreFactory> logger
        ) {
        _certificateStore = certificateStore;
        _listCertificateServiceFactory = listCertificateServiceFactory.ToList();
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public ICertificateStore? CreateCertificateStore(CertificateStoreOptions factoryOptions) {
        foreach (var factory in this._listCertificateServiceFactory) {
            var certificateService = factory.CreateCertificateStore(factoryOptions);
            if (certificateService != null) {
                _certificateStore.CertificateServices.Add(certificateService);
                return certificateService;
            }
        }
        return null;
    }
}
