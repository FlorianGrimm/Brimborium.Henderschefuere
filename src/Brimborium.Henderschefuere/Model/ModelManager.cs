
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;

namespace Brimborium.Henderschefuere.Model;

public sealed class HfModelManager {
    private HfConfigurationManager? _ConfigurationManager;
    private HfEndpointDataSource? _EndpointDataSource;
    private IDisposable? _EndpointDataSourceChange;
    private RequestDelegate? _Pipeline;
    private HfRootSnapshot? _Snapshot;
    private HfRootModel? _RootModel;

    public HfModelManager() {
    }

    internal void SetConfigurationManager(HfConfigurationManager configurationManager) {
        if (ReferenceEquals(this._ConfigurationManager, configurationManager)) { return; }

        this._ConfigurationManager = configurationManager;
        ChangeToken.OnChange(
            () => this._ConfigurationManager.GetSnapshotChangeToken(),
            () => this.OnConfigurationChanged()
            );
    }


    internal void SetEndpointDataSource(HfEndpointDataSource endpointDataSource, RequestDelegate pipeline) {
        if (ReferenceEquals(this._EndpointDataSource, endpointDataSource)) { return; }
        if (ReferenceEquals(this._Pipeline, pipeline)) { return; }


        this._EndpointDataSource = endpointDataSource;
        this._Pipeline = pipeline;
        if (this._ConfigurationManager is not null) {
            this.OnConfigurationChanged();
        }
    }

    private void OnConfigurationChanged() {
        if (this._ConfigurationManager is null) { return; }
        if (this._EndpointDataSource is null) { return; }

        var snapshot = this._ConfigurationManager.GetSnapshot();
        var rootModel = this.GetHfRootModel(snapshot, this._RootModel ?? new HfRootModel());
        var endpoints = this.GetRoutingEndpoints(rootModel);
        this._Snapshot = snapshot;
        this._RootModel = rootModel;
        this._EndpointDataSource.UpdateEndpoints(endpoints.ToImmutableArray());
    }

    internal HfRootModel GetHfRootModel(HfRootSnapshot rootSnapshot, HfRootModel oldRootModel) {
        Dictionary<string, HfTunnelPair> dictTunnels = new();
        Dictionary<string, HfRoutePair> dictRoutes = new();
        Dictionary<string, HfClusterPair> dictClusters = new();
        {
            foreach (var tunnelModel in rootSnapshot.Tunnels.Values) {
                var tunnelState = (oldRootModel.Tunnels.TryGetValue(tunnelModel.Id, out var old)) ? old.TunnelState : new HfTunnelState();
                dictTunnels.Add(tunnelModel.Id, new(tunnelModel, tunnelState));
            }
            foreach (var routeModel in rootSnapshot.Routes.Values) {
                var routeState = (oldRootModel.Routes.TryGetValue(routeModel.Id, out var old)) ? old.RouteState : new HfRouteState();
                dictRoutes.Add(routeModel.Id, new(routeModel, routeState));
            }
            foreach (var clusterModel in rootSnapshot.Clusters.Values) {
                Dictionary<string, HfClusterDestinationPair> destinations = new();
                foreach (var destinationModel in clusterModel.Destinations.Values) {
                    destinations.Add(destinationModel.Id, new(destinationModel));
                }
                var clusterState = (oldRootModel.Clusters.TryGetValue(clusterModel.Id, out var old)) ? old.ClusterState : new HfClusterState();
                var clusterPair = new HfClusterPair(clusterModel, destinations.ToImmutableDictionary(), clusterState);
                dictClusters.Add(clusterModel.Id, clusterPair);
            }
        }
        {
            foreach (var route in dictRoutes.Values) {
                if (dictClusters.TryGetValue(route.RouteModel.ClusterId, out var hfClusterPair)) {
                    route.Cluster = hfClusterPair;
                }
            }
        }
        return new HfRootModel(dictTunnels.ToImmutableDictionary(), dictRoutes.ToImmutableDictionary(), dictClusters.ToImmutableDictionary());
    }

    internal List<Endpoint> GetRoutingEndpoints(HfRootModel rootModel) {
        List<Endpoint> result = new List<Endpoint>();
        if (this._Pipeline is not { } pipeline) { return result; }

        foreach (var routeState in rootModel.Routes.Values) {
            int order = 0;
            var endpointBuilder = new RouteEndpointBuilder(
                requestDelegate: this._Pipeline,
                routePattern: RoutePatternFactory.Parse(routeState.RouteModel.Match.Path),
                order: order
            ) {
                DisplayName = routeState.Id
            };
            endpointBuilder.Metadata.Add(routeState);
            var routeEndpoint = endpointBuilder.Build();
            result.Add(routeEndpoint);
        }
        return result;
    }
}
