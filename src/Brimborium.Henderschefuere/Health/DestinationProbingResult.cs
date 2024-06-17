// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Health;

/// <summary>
/// Result of a destination's active health probing.
/// </summary>
public readonly struct DestinationProbingResult {
    public DestinationProbingResult(DestinationState destination, HttpResponseMessage? response, Exception? exception) {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        Response = response;
        Exception = exception;
    }

    /// <summary>
    /// Probed destination.
    /// </summary>
    public DestinationState Destination { get; }

    /// <summary>
    /// Response recieved.
    /// It can be null in case of a failure.
    /// </summary>
    public HttpResponseMessage? Response { get; }

    /// <summary>
    /// Exception thrown during probing.
    /// It is null in case of a success.
    /// </summary>
    public Exception? Exception { get; }
}
