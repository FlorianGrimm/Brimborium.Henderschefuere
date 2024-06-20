// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Utilities;

public sealed class UnShortCitcuitOnce<T>
    where T : notnull {
    private readonly IServiceProvider _serviceProvider;
    private T? _value;
    private bool _resolved;

    public UnShortCitcuitOnce(
        IServiceProvider serviceProvider
        ) {
        this._serviceProvider = serviceProvider;
    }

    public T? GetService() {
        if (_resolved) {
            return _value;
        } else {
            _resolved = true;
            return _value = _serviceProvider.GetService<T>();
        }
    }

    public T GetRequiredService() {
        if (_resolved && _value is T value) {
            return value;
        } else {
            _resolved = true;
            return _value = _serviceProvider.GetRequiredService<T>();
        }
    }
}

public abstract class UnShortCitcuitOnceFunc<T>
    where T : notnull {
    protected readonly IServiceProvider _serviceProvider;
    private T? _value;
    private bool _resolved;

    public UnShortCitcuitOnceFunc(
        IServiceProvider serviceProvider
        ) {
        this._serviceProvider = serviceProvider;
    }

    public T GetService() {
        if (_resolved && _value is T value) {
            return value;
        } else {
            _resolved = true;
            return this._value = Resolve();
        }
    }

    protected virtual T Resolve() {
        return _serviceProvider.GetRequiredService<T>();
    }
}

public abstract class UnShortCitcuitOnceFuncQ<T>
    where T : notnull {
    protected readonly IServiceProvider _serviceProvider;
    private T? _value;
    private bool _resolved;

    public UnShortCitcuitOnceFuncQ(
        IServiceProvider serviceProvider
        ) {
        this._serviceProvider = serviceProvider;
    }

    public T? GetService() {
        if (_resolved) {
            return _value;
        } else {
            _resolved = true;
            return this._value = Resolve();
        }
    }

    protected virtual T? Resolve() {
        return _serviceProvider.GetService<T>();
    }
}
