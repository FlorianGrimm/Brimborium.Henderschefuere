namespace Brimborium.Henderschefuere.Tunnel;

public interface ITunnelAuthenticationConfigService {
    void Configure(SocketsHttpHandler socketsHttpHandler, TunnelAuthenticationConfig authentication);
}
