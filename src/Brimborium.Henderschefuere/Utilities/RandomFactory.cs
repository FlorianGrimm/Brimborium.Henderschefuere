// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Utilities;

/// <inheritdoc/>
internal sealed class RandomFactory : IRandomFactory
{
    /// <inheritdoc/>
    public Random CreateRandomInstance()
    {
        return Random.Shared;
    }
}
