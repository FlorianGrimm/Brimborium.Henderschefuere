namespace Brimborium.Henderschefuere.Tunnel;
internal interface IStreamCloseable {
    bool IsClosed { get; }
    void Abort();
}
