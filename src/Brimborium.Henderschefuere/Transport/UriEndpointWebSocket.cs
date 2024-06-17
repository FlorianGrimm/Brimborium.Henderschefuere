namespace Brimborium.Henderschefuere.Transport;

// This is a .NET 6 workaround for https://github.com/dotnet/aspnetcore/pull/40003 (it's fixed in .NET 7)
public sealed class UriEndpointWebSocket : IPEndPoint
{
    public Uri? Uri { get; }

    public UriEndpointWebSocket(Uri uri) :
        this(0, 0)
    {
        Uri = uri;
    }

    public UriEndpointWebSocket(long address, int port) : base(address, port)
    {
    }

    public static implicit operator UriEndpointWebSocket(Uri uri)
    {
        return new UriEndpointWebSocket(uri);
    }
}
