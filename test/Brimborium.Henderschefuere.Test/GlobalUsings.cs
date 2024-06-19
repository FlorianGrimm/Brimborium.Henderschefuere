global using Autofac.Core;
global using Autofac.Extras.Moq;
global using Moq;

global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Diagnostics.Tracing;
global using System.Diagnostics.CodeAnalysis;
global using System.IO.Pipelines;
global using System.Linq;
global using System.Net;
global using System.Text;
global using System.Threading.Tasks;
global using System.Net.WebSockets;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Connections;
global using Microsoft.AspNetCore.Connections.Features;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.Connections.Client;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.AspNetCore.TestHost;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Primitives;

global using Brimborium.Henderschefuere;
global using Brimborium.Henderschefuere.Configuration;
global using Brimborium.Henderschefuere.Delegation;
global using Brimborium.Henderschefuere.Forwarder;
global using Brimborium.Henderschefuere.Health;
global using Brimborium.Henderschefuere.Limits;
global using Brimborium.Henderschefuere.LoadBalancing;
global using Brimborium.Henderschefuere.Management;
global using Brimborium.Henderschefuere.Model;
global using Brimborium.Henderschefuere.ServiceDiscovery;
global using Brimborium.Henderschefuere.SessionAffinity;
global using Brimborium.Henderschefuere.Transforms;
global using Brimborium.Henderschefuere.Transforms.Builder;
global using Brimborium.Henderschefuere.Transport;
global using Brimborium.Henderschefuere.Tunnel;
global using Brimborium.Henderschefuere.Utilities;
global using Brimborium.Henderschefuere.WebSocketsTelemetry;


global using Xunit;