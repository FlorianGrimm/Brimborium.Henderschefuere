// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Routing;

/// <summary>
/// Represents request query parameter metadata used during routing.
/// </summary>
internal sealed class QueryParameterMetadata : IQueryParameterMetadata
{
    public QueryParameterMetadata(IReadOnlyList<QueryParameterMatcher> matchers)
    {
        Matchers = matchers?.ToArray() ?? throw new ArgumentNullException(nameof(matchers));
    }

    /// <inheritdoc/>
    public QueryParameterMatcher[] Matchers { get; }
}
