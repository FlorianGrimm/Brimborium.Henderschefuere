// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Routing;

/// <summary>
/// Represents request header metadata used during routing.
/// </summary>
internal interface IHeaderMetadata {
    /// <summary>
    /// One or more matchers to apply to the request headers.
    /// </summary>
    HeaderMatcher[] Matchers { get; }
}
