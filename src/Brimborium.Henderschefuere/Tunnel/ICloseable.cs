namespace Brimborium.Henderschefuere.Tunnel;
internal interface ICloseable {
    bool IsClosed { get; }
    void Abort();
}
