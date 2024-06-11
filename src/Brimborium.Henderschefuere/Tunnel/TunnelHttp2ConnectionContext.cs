namespace Brimborium.Henderschefuere.Tunnel;

public class Http2UriEndPoint(Uri uri, HfTunnelModel tunnel) : TunnelUriEndPoint(uri, tunnel) { }

internal sealed class TunnelHttp2ConnectionContext
    : ConnectionContext
    , IConnectionLifetimeFeature
    , IConnectionEndPointFeature
    , IConnectionItemsFeature
    , IConnectionIdFeature
    , IConnectionTransportFeature
    , IDuplexPipe {
    private readonly TaskCompletionSource _ExecutionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private TunnelHttp2ConnectionContext() {
        this.Transport = this;

        this.Features.Set<IConnectionIdFeature>(this);
        this.Features.Set<IConnectionTransportFeature>(this);
        this.Features.Set<IConnectionItemsFeature>(this);
        this.Features.Set<IConnectionEndPointFeature>(this);
        this.Features.Set<IConnectionLifetimeFeature>(this);
    }

    public Task ExecutionTask => this._ExecutionTcs.Task;

    public override string ConnectionId { get; set; } = Guid.NewGuid().ToString();

    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override IDictionary<object, object?> Items { get; set; } = new ConnectionItems();
    public override IDuplexPipe Transport { get; set; }

    public override EndPoint? LocalEndPoint { get; set; }

    public override EndPoint? RemoteEndPoint { get; set; }

    public PipeReader Input { get; set; } = default!;

    public PipeWriter Output { get; set; } = default!;

    public override CancellationToken ConnectionClosed { get; set; }

    public HttpResponseMessage HttpResponseMessage { get; set; } = default!;

    public override void Abort() {
        this.HttpResponseMessage.Dispose();

        this._ExecutionTcs.TrySetCanceled();

        this.Input.CancelPendingRead();
        this.Output.CancelPendingFlush();
    }

    public override void Abort(ConnectionAbortedException abortReason) {
        this.Abort();
    }

    public override ValueTask DisposeAsync() {
        this.Abort();

        return base.DisposeAsync();
    }

    private class HttpClientConnectionContextContent : HttpContent {
        private readonly TunnelHttp2ConnectionContext _ConnectionContext;

        public HttpClientConnectionContextContent(TunnelHttp2ConnectionContext connectionContext) {
            this._ConnectionContext = connectionContext;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) {
            this._ConnectionContext.Output = PipeWriter.Create(stream);

            // Immediately flush request stream to send headers
            // https://github.com/dotnet/corefx/issues/39586#issuecomment-516210081
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

            await this._ConnectionContext.ExecutionTask.ConfigureAwait(false);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context) {
            this._ConnectionContext.Output = PipeWriter.Create(stream);

            // Immediately flush request stream to send headers
            // https://github.com/dotnet/corefx/issues/39586#issuecomment-516210081
            await stream.FlushAsync().ConfigureAwait(false);

            await this._ConnectionContext.ExecutionTask.ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length) {
            length = -1;
            return false;
        }
    }


    internal sealed class Factory : ITunnelConnectionListenerFactory {
        private HttpMessageInvoker _Invoker;

        public Factory() {
            this._Invoker = new HttpMessageInvoker(
                new SocketsHttpHandler {
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan
                });

        }
        public Type GetEndPointType() => typeof(Http2UriEndPoint);

        public async ValueTask<ConnectionContext> ConnectAsync(TunnelUriEndPoint endPoint, CancellationToken cancellationToken = default) {
            var request = new HttpRequestMessage(HttpMethod.Post, endPoint.Uri) {
                Version = new Version(2, 0)
            };
            // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");
            var connection = new TunnelHttp2ConnectionContext();
            request.Content = new HttpClientConnectionContextContent(connection);
            var response = await this._Invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
            connection.HttpResponseMessage = response;
            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            connection.Input = PipeReader.Create(responseStream);

            return connection;
        }

    }

}
