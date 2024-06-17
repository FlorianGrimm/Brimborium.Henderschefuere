// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Utilities;

internal static class TaskUtilities
{
    internal static readonly Task<bool> TrueTask = Task.FromResult(true);
    internal static readonly Task<bool> FalseTask = Task.FromResult(false);
}
