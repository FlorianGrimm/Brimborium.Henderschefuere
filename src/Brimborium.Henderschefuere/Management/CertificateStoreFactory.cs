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
    private readonly ILogger _logger;

    public CertificateStoreFactory(
        CertificateStore certificateStore,
        IEnumerable<ICertificateStoreFactory> listCertificateServiceFactory,
        ILogger<CertificateStoreFactory> logger
        ) {
        _certificateStore = certificateStore;
        _listCertificateServiceFactory = listCertificateServiceFactory.ToList();
        _logger = logger;
    }

    public ICertificateStore? CreateCertificateStore(CertificateStoreOptions options) {
        foreach (var factory in this._listCertificateServiceFactory) {
            var certificateService = factory.CreateCertificateStore(options);
            if (certificateService != null) {
                _certificateStore.CertificateServices.Add(certificateService);
                return certificateService;
            }
        }
        return null;
    }
}

public sealed class OptionalCertificateStoreFactory(IServiceProvider serviceProvider) : UnShortCitcuitOnceFuncQ<CertificateStoreFactory>(serviceProvider) {
}