namespace Brimborium.Henderschefuere.Transport;

internal sealed class IncrementalDelay(int increment = 500, int maximum = 15 * 60 * 1000) {
    private readonly int _increment = increment;
    private readonly int _maximum = maximum;
    private int _current = 0;

    public void Reset() {
        _current = 0;
    }

    public async Task Delay(CancellationToken cancellationToken) {
        if (_current < _maximum) {
            _current += _increment;
            if (_maximum < _current) { _current = _maximum; }
        }
        await Task.Delay(_current, cancellationToken);
    }
}
