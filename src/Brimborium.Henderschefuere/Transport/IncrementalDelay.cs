namespace Brimborium.Henderschefuere.Transport;

internal sealed class IncrementalDelay(int increment = 500, int maximum = 15 * 60 * 1000) {
    internal int Increment = increment;
    internal int Maximum = maximum;
    internal int Current = 0;
    private int _CountWait = 0;
    public void Reset() {
        Current = 0;
    }

    public async Task Delay(CancellationToken cancellationToken) {
        if (Current < Maximum) {
            Current += (Increment / (_CountWait + 1));
            if (Maximum < Current) { Current = Maximum; }
        }
        System.Threading.Interlocked.Increment(ref _CountWait);
        try {
            await Task.Delay(Current, cancellationToken);
        } finally {
            System.Threading.Interlocked.Decrement(ref _CountWait);
        }
    }
}
