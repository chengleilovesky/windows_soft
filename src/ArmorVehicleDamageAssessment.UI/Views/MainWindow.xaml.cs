using System;
using System.Windows;
using ArmorVehicleDamageAssessment.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.UI.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;

    public MainWindow(ILogger<MainWindow> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeComponent();

        _logger.LogInformation("主窗口初始化完成");

        // 默认显示算例管理页面
        ShowSimulationCaseView();
    }

    private void SimulationCaseButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSimulationCaseView();
    }

    private void ShowSimulationCaseView()
    {
        try
        {
            // 隐藏所有视图
            WelcomeGrid.Visibility = Visibility.Collapsed;

            // 显示算例管理视图
            SimulationCaseView.Visibility = Visibility.Visible;

            _logger.LogDebug("切换到算例管理视图");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换到算例管理视图时发生错误");
            MessageBox.Show($"切换视图失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
