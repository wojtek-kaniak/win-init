using System.Diagnostics;

namespace WinInit;

public class InitService : IHostedService
{
    public InitService(ILogger<InitService> logger, IHostApplicationLifetime lifetime)
    {
        Logger = logger;
        Lifetime = lifetime;
    }

    private Process? InitProcess { get; set; }
    private ILogger<InitService> Logger { get; init; }
    private IHostApplicationLifetime Lifetime { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        async Task Start()
        {
            Logger.LogInformation(0, "Starting");

            string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "wininit");
            Directory.CreateDirectory(path);

            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardOutput = false
            };
            startInfo.ArgumentList.Add(Path.Join(path, "init.ps1"));

            InitProcess = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start");

            await (InitProcess?.WaitForExitAsync(default) ?? Task.CompletedTask);
            Lifetime.StopApplication();
        }

        try
        {
            await Start();
        }
        catch (Exception e)
        {
            Logger.LogCritical(2, e, "Exception occured: ");
            Lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(1, "Stopping");

        try
        {
            InitProcess?.Kill(true);
        }
        catch (InvalidOperationException)
        {
            // Process already exited
        }

        return Task.CompletedTask;
    }
}
