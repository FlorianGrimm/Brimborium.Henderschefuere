// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Routing;

/// <summary>
/// Represents request header metadata used during routing.
/// </summary>
internal sealed class HeaderMetadata : IHeaderMetadata
{
    public HeaderMetadata(IReadOnlyList<HeaderMatcher> matchers)
    {
        Matchers = matchers?.ToArray() ?? throw new ArgumentNullException(nameof(matchers));
    }

    /// <inheritdoc/>
    public HeaderMatcher[] Matchers { get; }
}
