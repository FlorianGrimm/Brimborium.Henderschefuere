// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using Brimborium.Henderschefuere.Utilities;

namespace Brimborium.Tests.Common;

public class TestRandomFactory : IRandomFactory {
    public TestRandom Instance { get; set; }

    public Random CreateRandomInstance() {
        return Instance;
    }
}
