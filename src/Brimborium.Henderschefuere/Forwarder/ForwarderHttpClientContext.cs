// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Forwarder;

/// <summary>
/// Contains the old and the new HTTP client configurations.
/// </summary>
public sealed class ForwarderHttpClientContext {
    /// <summary>
    /// Id of a <see cref="ClusterModel"/> HTTP client belongs to.
    /// </summary>
    public string ClusterId { get; init; } = default!;

    /// <summary>
    /// Old <see cref="HttpClientConfig"/> instance
    /// from which the <see cref="OldClient"/> was created.
    /// Can be empty if a client is getting constructed for the first time.
    /// </summary>
    public HttpClientConfig OldConfig { get; init; } = default!;

    /// <summary>
    /// Old metadata instance from which the <see cref="OldClient"/> was created, if any.
    /// </summary>
    public IReadOnlyDictionary<string, string>? OldMetadata { get; init; }

    /// <summary>
    /// Old <see cref="HttpMessageInvoker"/> instance.
    /// Can be null if a client is getting constructed for the first time.
    /// </summary>
    public HttpMessageInvoker? OldClient { get; init; }

    /// <summary>
    /// New <see cref="HttpClientConfig"/> instance
    /// specifying the settings for a new client.
    /// </summary>
    public HttpClientConfig NewConfig { get; init; } = default!;

    /// <summary>
    /// New metadata instance used for a new client construction, if any.
    /// </summary>
    public IReadOnlyDictionary<string, string>? NewMetadata { get; init; }
}
