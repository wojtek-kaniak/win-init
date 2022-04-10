using WinInit;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<InitService>();
    })
    .Build();

await host.RunAsync();
