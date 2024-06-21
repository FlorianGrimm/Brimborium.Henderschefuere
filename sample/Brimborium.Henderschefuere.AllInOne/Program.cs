using Brimborium.Henderschefuere.Transport;

using System.Diagnostics;

namespace Brimborium.Henderschefuere.AllInOne;

public class Program {
    public static async Task Main(string[] args) {
        await StartServers();
    }

    public static async Task StartServers() {
        CancellationTokenSource cts = new();
        List<ServerBase> listServer = new();

        listServer.Add(new ServerFrontend("appsettings.server1FE.json"));
        listServer.Add(new ServerFrontend("appsettings.server2FE.json"));
        listServer.Add(new ServerBackend("appsettings.server3BE.json"));
        listServer.Add(new ServerBackend("appsettings.server4BE.json"));
        listServer.Add(new ServerAPI("appsettings.server5API.json"));
        listServer.Add(new ServerAPI("appsettings.server6API.json"));

        foreach (var server in listServer) {
            server.Build();
        }
        foreach (var server in listServer) {
            await server.Run(() => { cts.Cancel(); }, cts.Token);
        }
        var listRunTask = listServer.Select(server => server.RunTask!).ToList();
        listRunTask.Add(runTests());
        await Task.WhenAll(listRunTask);

#if false
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseAuthorization();
        //app.MapConnections
        app.Run();
#endif
    }

    private static async Task runTests() {

        (string url, long duration)[] listUrl = [
            ("https://localhost:5001/Frontend", 0),
            ("https://localhost:5002/Frontend", 0),
            ("https://localhost:5001/Backend", 0),
            ("https://localhost:5002/Backend", 0),
            ("https://localhost:5001/alpha/API", 0),
            ("https://localhost:5002/beta/API", 0),
            ];
        Dictionary<string, int> dict = new();
        var cntloop = 2;
        for (var loop = 1; loop <= cntloop; loop++) {
            System.Console.Out.WriteLine($"{loop} / {cntloop}");

            for (int index = 0; index < listUrl.Length; index++) {
                Stopwatch sw = Stopwatch.StartNew();
                List<(HttpClient httpClient, Task<string> taskGetString)> listTask = new();

                for (int innerloop = 0; innerloop < 20; innerloop++) {
                    HttpClient client = new HttpClient(new SocketsHttpHandler());
                    var taskGetString = client.GetStringAsync(listUrl[index].url);
                    listTask.Add((client, taskGetString));
                }

                foreach (var (client, taskGetString) in listTask) {
                    var text = await taskGetString;
                    if (dict.TryGetValue(text, out var count)) {
                        dict[text] = count + 1;
                    } else {
                        dict[text] = 1;
                    }
                    client.Dispose();
                }
                listUrl[index].duration += sw.ElapsedMilliseconds;
            }
        }

        foreach (var (url, duration) in listUrl) {
            System.Console.WriteLine($"{url} - {duration}");
        }

        foreach (var (text, count) in dict) {
            System.Console.WriteLine($"{count} - {text}");
        }
    }
}

public abstract class ServerBase {
    private string _AppsettingsJsonFile;
    private WebApplicationBuilder _Builder;

    protected ServerBase(string appsettingsJsonFile) {
        var builder = this._Builder = WebApplication.CreateBuilder();
        builder.Configuration.AddJsonFile(this._AppsettingsJsonFile = appsettingsJsonFile, false, true);
        //builder.Logging.AddConsole();
        builder.Logging.ClearProviders();
    }

    public WebApplication? App { get; private set; }
    public Task? RunTask { get; private set; }

    public void Build() {
        this.ConfigureBuilder(this._Builder);
        var app = this._Builder.Build();
        this.App = app;
        this.ConfigureApp(this._Builder, app);
    }

    public virtual void ConfigureBuilder(WebApplicationBuilder builder) {
    }

    public virtual void ConfigureApp(WebApplicationBuilder builder, WebApplication app) {
    }

    public virtual async Task Run(Action handleStopping, CancellationToken stopToken) {
        if (this.App is null) { this.Build(); }
        var app = this.App;
        if (app is null) { throw new InvalidOperationException("this.App is null"); }
        TaskCompletionSource tcsStarting = new();
        app.Lifetime.ApplicationStarted.Register(() => {
            Console.WriteLine($"{this._AppsettingsJsonFile} - Application started.");
            tcsStarting.TrySetResult();
        });
        app.Lifetime.ApplicationStopping.Register(() => {
            Console.WriteLine($"{this._AppsettingsJsonFile} - Application stopping...");
            handleStopping();
        });
        this.RunTask = app.RunAsync(stopToken);
        await tcsStarting.Task;
    }
}

public sealed class ServerFrontend : ServerBase {
    public ServerFrontend(string appsettingsJsonFile) : base(appsettingsJsonFile) {
    }

    public override void ConfigureBuilder(WebApplicationBuilder builder) {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetRequiredSection("ReverseProxy"))
            .AddTunnelServices()
            //.UseTunnelTransport(builder)
            ;
    }

    public override void ConfigureApp(WebApplicationBuilder builder, WebApplication app) {
        app.MapReverseProxy();

        app.MapGet("/Frontend", (HttpContext context) => {
            var urls = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Urls");
            return $"Backend {urls} - {context.Request.Host} - {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
        });
    }
}

public sealed class ServerBackend : ServerBase {
    public ServerBackend(string appsettingsJsonFile) : base(appsettingsJsonFile) {
    }

    public override void ConfigureBuilder(WebApplicationBuilder builder) {
        builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetRequiredSection("ReverseProxy"))
            //.AddTunnelServices()
            .UseTunnelTransport(builder)
            ;

    }

    public override void ConfigureApp(WebApplicationBuilder builder, WebApplication app) {
        app.UseWebSockets();
        app.MapControllers();
        app.MapReverseProxy();
        app.MapGet("/Backend", (HttpContext context) => {
            var urls = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Urls");
            return $"Backend {urls} - {context.Request.Host} - {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
        });
    }
}

public sealed class ServerAPI : ServerBase {
    public ServerAPI(string appsettingsJsonFile) : base(appsettingsJsonFile) {
    }

    public override void ConfigureBuilder(WebApplicationBuilder builder) {
        builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public override void ConfigureApp(WebApplicationBuilder builder, WebApplication app) {
        app.UseWebSockets();
        app.MapControllers();
        app.MapGet("/API", (HttpContext context) => {
            var urls = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Urls");
            return $"API {urls} - {context.Request.Host} - {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
        });
        app.MapGet("/alpha/API", (HttpContext context) => {
            var urls = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Urls");
            return $"API {urls} - {context.Request.Host} - {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
        });
        app.MapGet("/beta/API", (HttpContext context) => {
            var urls = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Urls");
            return $"API {urls} - {context.Request.Host} - {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}";
        });
    }
}
