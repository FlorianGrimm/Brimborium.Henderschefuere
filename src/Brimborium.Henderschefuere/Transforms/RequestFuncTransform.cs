// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Transforms;

/// <summary>
/// A request transform that runs the given Func.
/// </summary>
public class RequestFuncTransform : RequestTransform
{
    private readonly Func<RequestTransformContext, ValueTask> _func;

    public RequestFuncTransform(Func<RequestTransformContext, ValueTask> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    /// <inheritdoc/>
    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        return _func(context);
    }
}
