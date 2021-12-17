using FundTransfer.IoC;
using FundTransfer.Worker;
using Serilog;
using Serilog.Events;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(hostContext.Configuration)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .WriteTo.Console()
        .CreateLogger();
        DependencyInjector.InjectDependencies(services, hostContext.Configuration);
    })
    .Build();

await host.RunAsync();