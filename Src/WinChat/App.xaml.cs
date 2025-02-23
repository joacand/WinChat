using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Channels;
using System.Windows;
using WinChat.Infrastructure;
using WinChat.Infrastructure.Repository;
using WinChat.Services;
using WinChat.ViewModels;
using WinChat.Views;

namespace WinChat;

public partial class App : Application
{
    private readonly IHost _host;
    private readonly IConfiguration _configuration;

    public App()
    {
        _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            })
            .Build();

        Ioc.Default.ConfigureServices(_host.Services);
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var functionCallChannel = Channel.CreateUnbounded<FunctionCallContent>();
        services.AddSingleton(functionCallChannel);
        services.AddSingleton(_configuration);
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ConfigurationViewModel>();
        services.AddSingleton<ColorSettings>();
        services.AddTransient<SoundService>();
        services.RegisterInfrastructureServices();

        var dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db");
        if (!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }
        var dbPath = Path.Combine(dbFolder, "winchat.db");
        var connectionString = $"Data Source={dbPath}";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString,
                x => x.MigrationsAssembly("WinChat.Infrastructure"));
        });
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _host.Start();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
