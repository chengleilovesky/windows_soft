using ArmorVehicleDamageAssessment.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;

namespace ArmorVehicleDamageAssessment.UI.Views;

/// <summary>
/// SimulationCaseView.xaml 的交互逻辑
/// </summary>
public partial class SimulationCaseView : UserControl
{
    private readonly ILogger<SimulationCaseView> _logger;

    public SimulationCaseView()
    {
        InitializeComponent();
        
        // 获取服务
        if (System.Windows.Application.Current is App app)
        {
            var serviceProvider = (app as dynamic)?._host?.Services;
            if (serviceProvider != null)
            {
                _logger = serviceProvider.GetRequiredService<ILogger<SimulationCaseView>>();
                
                // 设置DataContext
                var viewModel = serviceProvider.GetRequiredService<SimulationCaseViewModel>();
                DataContext = viewModel;

                _logger.LogInformation("算例管理视图初始化完成");
            }
            else
            {
                throw new InvalidOperationException("无法获取服务提供者");
            }
        }
        else
        {
            throw new InvalidOperationException("应用程序实例无效");
        }
    }

    public SimulationCaseView(SimulationCaseViewModel viewModel, ILogger<SimulationCaseView> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        
        _logger.LogInformation("算例管理视图初始化完成");
    }
}
