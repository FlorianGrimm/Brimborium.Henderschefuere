// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Transforms.Builder;

internal sealed class ActionTransformProvider : ITransformProvider {
    private readonly Action<TransformBuilderContext> _action;

    public ActionTransformProvider(Action<TransformBuilderContext> action) {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Apply(TransformBuilderContext transformBuildContext) {
        _action(transformBuildContext);
    }

    public void ValidateRoute(TransformRouteValidationContext context) {
    }

    public void ValidateCluster(TransformClusterValidationContext context) {
    }
}
