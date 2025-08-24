using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ArmorVehicleDamageAssessment.Common.Interfaces;
using ArmorVehicleDamageAssessment.Core.Interfaces;
using ArmorVehicleDamageAssessment.Core.Services;
using ArmorVehicleDamageAssessment.Data.Context;
using ArmorVehicleDamageAssessment.Data.Repositories;
using ArmorVehicleDamageAssessment.UI.ViewModels;
using ArmorVehicleDamageAssessment.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // 创建日志目录
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // 记录启动日志
            var startupLog = Path.Combine(logDir, "startup.log");
            File.AppendAllText(startupLog, $"{DateTime.Now}: 应用程序开始启动\n");

            // 构建Host
            _host = CreateHostBuilder().Build();
            File.AppendAllText(startupLog, $"{DateTime.Now}: Host创建成功\n");

            // 确保数据库创建
            await EnsureDatabaseCreatedAsync();
            File.AppendAllText(startupLog, $"{DateTime.Now}: 数据库初始化成功\n");

            // 启动Host
            await _host.StartAsync();
            File.AppendAllText(startupLog, $"{DateTime.Now}: Host启动成功\n");

            // 显示主窗口
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            File.AppendAllText(startupLog, $"{DateTime.Now}: 主窗口显示成功\n");

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            // 详细的错误日志
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            var errorLog = Path.Combine(logDir, "error.log");
            var errorDetails = $"{DateTime.Now}: 启动失败\n" +
                              $"错误消息: {ex.Message}\n" +
                              $"堆栈跟踪: {ex.StackTrace}\n" +
                              $"内部异常: {ex.InnerException?.ToString() ?? "无"}\n" +
                              $"=====================================\n";
            
            File.AppendAllText(errorLog, errorDetails);

            MessageBox.Show($"应用程序启动失败：\n{ex.Message}\n\n详细信息请查看：{errorLog}", 
                "启动错误", MessageBoxButton.OK, MessageBoxImage.Error);
            
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // 配置文件路径
                var basePath = Directory.GetCurrentDirectory();
                config.SetBasePath(basePath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddNLog("NLog.config");
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((context, services) =>
            {
                // 数据库配置
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // 默认数据库路径
                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "ArmorVehicleDamageAssessment", "Data", "ArmorVehicleAssessment.db");
                    var directory = Path.GetDirectoryName(dbPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory!);
                    }
                    connectionString = $"Data Source={dbPath};";
                }

                services.AddDbContext<ArmorVehicleDbContext>(options =>
                {
                    options.UseSqlite(connectionString);
                    options.EnableSensitiveDataLogging(true);
                    options.EnableDetailedErrors(true);
                });

                // 注册仓储
                services.AddScoped<ISimulationCaseRepository, SimulationCaseRepository>();

                // 注册服务
                services.AddScoped<ISimulationCaseService, SimulationCaseService>();

                // 注册ViewModels
                services.AddTransient<SimulationCaseViewModel>();

                // 注册Views
                services.AddTransient<MainWindow>();
                services.AddTransient<SimulationCaseView>();
            });
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        if (_host == null) return;

        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArmorVehicleDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

        try
        {
            logger.LogInformation("开始创建数据库...");
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("数据库创建完成");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "数据库创建失败");
            throw;
        }
    }
}
