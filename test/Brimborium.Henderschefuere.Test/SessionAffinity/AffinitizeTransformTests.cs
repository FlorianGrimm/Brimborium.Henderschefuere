// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Brimborium.Henderschefuere.Configuration;
using Brimborium.Henderschefuere.Forwarder;
using Brimborium.Henderschefuere.Model;
using Brimborium.Henderschefuere.Transforms;

namespace Brimborium.Henderschefuere.SessionAffinity.Tests;

public class AffinitizeTransformTests {
    [Fact]
    public async Task ApplyAsync_InvokeAffinitizeRequest() {
        var cluster = GetCluster();
        var destination = cluster.Destinations.Values.First();
        var provider = new Mock<ISessionAffinityPolicy>(MockBehavior.Strict);
        provider
            .Setup(p => p.AffinitizeResponseAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<ClusterState>(),
                It.IsNotNull<SessionAffinityConfig>(),
                It.IsAny<DestinationState>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        var transform = new AffinitizeTransform(provider.Object);

        var context = new DefaultHttpContext();
        context.Features.Set<IReverseProxyFeature>(new ReverseProxyFeature() {
            Cluster = cluster.Model,
            Route = new RouteModel(new RouteConfig(), cluster, HttpTransformer.Default),
            ProxiedDestination = destination,
        });

        var transformContext = new ResponseTransformContext() {
            HttpContext = context,
        };
        await transform.ApplyAsync(transformContext);

        provider.Verify();
    }

    internal ClusterState GetCluster() {
        var cluster = new ClusterState("cluster-1");
        cluster.Destinations.GetOrAdd("dest-A", id => new DestinationState(id));
        cluster.Model = new ClusterModel(new ClusterConfig {
            SessionAffinity = new SessionAffinityConfig {
                Enabled = true,
                Policy = "Policy-B",
                FailurePolicy = "Policy-1",
                AffinityKeyName = "Key1"
            }
        },
        new HttpMessageInvoker(new Mock<HttpMessageHandler>().Object));

        var destinations = cluster.Destinations.Values.ToList();
        cluster.DestinationsState = new ClusterDestinationsState(destinations, destinations);
        return cluster;
    }
}
