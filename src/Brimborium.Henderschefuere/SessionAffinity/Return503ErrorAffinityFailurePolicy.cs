// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.SessionAffinity;

internal sealed class Return503ErrorAffinityFailurePolicy : IAffinityFailurePolicy {
    public string Name => SessionAffinityConstants.FailurePolicies.Return503Error;

    public Task<bool> Handle(HttpContext context, ClusterState cluster, AffinityStatus affinityStatus) {
        if (affinityStatus == AffinityStatus.OK
            || affinityStatus == AffinityStatus.AffinityKeyNotSet) {
            throw new InvalidOperationException($"{nameof(Return503ErrorAffinityFailurePolicy)} is called to handle a successful request's affinity status {affinityStatus}.");
        }

        context.Response.StatusCode = 503;
        return TaskUtilities.FalseTask;
    }
}
