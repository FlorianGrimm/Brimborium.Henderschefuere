
namespace Brimborium.Henderschefuere.Configuration;
public class HfEndpointDataSource : EndpointDataSource {
    private ImmutableArray<Endpoint> _Endpoints = ImmutableArray<Endpoint>.Empty;
    private CancellationTokenSource _EndpointsChangeSource;
    private IChangeToken _EndpointsChangeToken;

    public HfEndpointDataSource()
    {
        this._EndpointsChangeSource = new ();
        this._EndpointsChangeToken = new CancellationChangeToken(this._EndpointsChangeSource.Token);
    }

    public override IReadOnlyList<Endpoint> Endpoints => this._Endpoints;

    public override IChangeToken GetChangeToken() => this._EndpointsChangeToken;

    public void UpdateEndpoints(ImmutableArray<Endpoint> endpoints) {
        lock (this) {
            var oldCancellationTokenSource = this._EndpointsChangeSource;
            this._Endpoints = endpoints;
            this._EndpointsChangeSource = new CancellationTokenSource();
            this._EndpointsChangeToken = new CancellationChangeToken(this._EndpointsChangeSource.Token);
            System.Threading.Interlocked.MemoryBarrier();
            oldCancellationTokenSource?.Cancel();
        }
    }}
