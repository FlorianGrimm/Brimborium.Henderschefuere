﻿namespace Brimborium.Henderschefuere.Configuration.TunnelValidators;

/// <summary>
/// Provides method to validate tunnel configuration.
/// </summary>
public interface ITunnelValidator {
    /// <summary>
    /// Perform validation on a tunnel by adding exceptions to the provided collection.
    /// </summary>
    /// <param name="tunnelConfig">tunnel configuration to validate</param>
    /// <param name="errors">Collection of all validation exceptions</param>
    /// <returns>A ValueTask representing the asynchronous validation operation.</returns>
    public ValueTask ValidateAsync(TunnelConfig tunnelConfig, IList<Exception> errors);
}
