global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Collections.Immutable;
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
global using Microsoft.AspNetCore.Routing;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Primitives;

global using Brimborium.Henderschefuere;
global using Brimborium.Henderschefuere.Configuration;
global using Brimborium.Henderschefuere.Model;

//