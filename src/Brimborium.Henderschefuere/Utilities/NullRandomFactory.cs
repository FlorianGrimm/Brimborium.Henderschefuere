// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Utilities;

internal sealed class NullRandomFactory : IRandomFactory {
    public Random CreateRandomInstance() {
        throw new NotImplementedException();
    }
}
