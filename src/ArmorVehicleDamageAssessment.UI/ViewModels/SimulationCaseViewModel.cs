using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ArmorVehicleDamageAssessment.Common.Helpers;
using ArmorVehicleDamageAssessment.Common.Models;
using ArmorVehicleDamageAssessment.Core.Interfaces;
using ArmorVehicleDamageAssessment.Core.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.UI.ViewModels;

/// <summary>
/// 算例管理ViewModel
/// </summary>
public partial class SimulationCaseViewModel : ViewModelBase
{
    private readonly ISimulationCaseService _simulationCaseService;

    public SimulationCaseViewModel(
        ISimulationCaseService simulationCaseService,
        ILogger<SimulationCaseViewModel> logger) : base(logger)
    {
        _simulationCaseService = simulationCaseService ?? throw new ArgumentNullException(nameof(simulationCaseService));

        // 初始化集合
        SimulationCases = new ObservableCollection<SimulationCaseDto>();
        SelectedCases = new ObservableCollection<SimulationCaseDto>();

        // 监听集合变化以更新HasItems属性
        SimulationCases.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasItems));

        // 初始化筛选选项
        SimulationTypes = EnumHelper.GetDisplayItems<SimulationType>();
        SimulationStatuses = EnumHelper.GetDisplayItems<SimulationStatus>();

        // 初始化命令
        InitializeCommands();

        // 加载数据
        _ = LoadDataAsync();
    }

    #region 属性

    /// <summary>
    /// 算例列表
    /// </summary>
    public ObservableCollection<SimulationCaseDto> SimulationCases { get; }

    /// <summary>
    /// 选中的算例列表
    /// </summary>
    public ObservableCollection<SimulationCaseDto> SelectedCases { get; }

    /// <summary>
    /// 当前选中的算例
    /// </summary>
    private SimulationCaseDto? _selectedCase;
    public SimulationCaseDto? SelectedCase
    {
        get => _selectedCase;
        set => SetProperty(ref _selectedCase, value);
    }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    private string _searchKeyword = string.Empty;
    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            SetProperty(ref _searchKeyword, value);
            _ = SearchAsync();
        }
    }

    /// <summary>
    /// 选中的类型筛选
    /// </summary>
    private EnumDisplayItem<SimulationType>? _selectedType;
    public EnumDisplayItem<SimulationType>? SelectedType
    {
        get => _selectedType;
        set
        {
            SetProperty(ref _selectedType, value);
            _ = SearchAsync();
        }
    }

    /// <summary>
    /// 选中的状态筛选
    /// </summary>
    private EnumDisplayItem<SimulationStatus>? _selectedStatus;
    public EnumDisplayItem<SimulationStatus>? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            SetProperty(ref _selectedStatus, value);
            _ = SearchAsync();
        }
    }

    /// <summary>
    /// 当前页码
    /// </summary>
    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    /// <summary>
    /// 页面大小
    /// </summary>
    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    /// <summary>
    /// 总页数
    /// </summary>
    private int _totalPages;
    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    /// <summary>
    /// 总记录数
    /// </summary>
    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    /// <summary>
    /// 仿真类型选项
    /// </summary>
    public List<EnumDisplayItem<SimulationType>> SimulationTypes { get; }

    /// <summary>
    /// 仿真状态选项
    /// </summary>
    public List<EnumDisplayItem<SimulationStatus>> SimulationStatuses { get; }

    /// <summary>
    /// 是否可以批量删除
    /// </summary>
    public bool CanBatchDelete => SelectedCases.Count > 0;

    /// <summary>
    /// 是否有数据项
    /// </summary>
    public bool HasItems => SimulationCases.Count > 0;

    #endregion

    #region 命令

    /// <summary>
    /// 搜索命令
    /// </summary>
    public IRelayCommand SearchCommand { get; private set; } = null!;

    /// <summary>
    /// 刷新命令
    /// </summary>
    public IRelayCommand RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// 新增算例命令
    /// </summary>
    public IRelayCommand CreateCaseCommand { get; private set; } = null!;

    /// <summary>
    /// 编辑算例命令
    /// </summary>
    public IRelayCommand<SimulationCaseDto> EditCaseCommand { get; private set; } = null!;

    /// <summary>
    /// 删除算例命令
    /// </summary>
    public IRelayCommand<SimulationCaseDto> DeleteCaseCommand { get; private set; } = null!;

    /// <summary>
    /// 批量删除命令
    /// </summary>
    public IRelayCommand BatchDeleteCommand { get; private set; } = null!;

    /// <summary>
    /// 查看算例详情命令
    /// </summary>
    public IRelayCommand<SimulationCaseDto> ViewCaseCommand { get; private set; } = null!;

    /// <summary>
    /// 打开工作路径命令
    /// </summary>
    public IRelayCommand<SimulationCaseDto> OpenWorkingPathCommand { get; private set; } = null!;

    /// <summary>
    /// 上一页命令
    /// </summary>
    public IRelayCommand PreviousPageCommand { get; private set; } = null!;

    /// <summary>
    /// 下一页命令
    /// </summary>
    public IRelayCommand NextPageCommand { get; private set; } = null!;

    /// <summary>
    /// 跳转页面命令
    /// </summary>
    public IRelayCommand<int> GotoPageCommand { get; private set; } = null!;

    #endregion

    #region 私有方法

    /// <summary>
    /// 初始化命令
    /// </summary>
    private void InitializeCommands()
    {
        SearchCommand = new RelayCommand(() => _ = SearchAsync());
        RefreshCommand = new RelayCommand(() => _ = LoadDataAsync());
        CreateCaseCommand = new RelayCommand(CreateCase);
        EditCaseCommand = new RelayCommand<SimulationCaseDto>(EditCase);
        DeleteCaseCommand = new RelayCommand<SimulationCaseDto>(DeleteCase);
        BatchDeleteCommand = new RelayCommand(BatchDelete, () => CanBatchDelete);
        ViewCaseCommand = new RelayCommand<SimulationCaseDto>(ViewCase);
        OpenWorkingPathCommand = new RelayCommand<SimulationCaseDto>(OpenWorkingPath);
        PreviousPageCommand = new RelayCommand(PreviousPage, () => CurrentPage > 1);
        NextPageCommand = new RelayCommand(NextPage, () => CurrentPage < TotalPages);
        GotoPageCommand = new RelayCommand<int>(GotoPage);

        // 监听选中项变化以更新批量删除命令状态
        SelectedCases.CollectionChanged += (_, _) => BatchDeleteCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new SimulationCasePagedRequest
            {
                PageNumber = CurrentPage,
                PageSize = PageSize,
                SearchKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                Type = SelectedType?.Value,
                Status = SelectedStatus?.Value
            };

            var result = await _simulationCaseService.GetPagedListAsync(request);

            // 更新UI
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SimulationCases.Clear();
                foreach (var item in result.Items)
                {
                    SimulationCases.Add(item);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = result.PageNumber;

                // 更新分页命令状态
                PreviousPageCommand.NotifyCanExecuteChanged();
                NextPageCommand.NotifyCanExecuteChanged();
            });

            Logger.LogInformation("成功加载算例数据，总数：{TotalCount}，当前页：{CurrentPage}", 
                result.TotalCount, result.PageNumber);

        }, "加载算例数据中...");
    }

    /// <summary>
    /// 搜索
    /// </summary>
    private async Task SearchAsync()
    {
        CurrentPage = 1; // 重置到第一页
        await LoadDataAsync();
    }

    /// <summary>
    /// 创建算例
    /// </summary>
    private void CreateCase()
    {
        Logger.LogInformation("打开创建算例对话框");
        // TODO: 打开创建算例对话框
        MessageBox.Show("创建算例功能待实现", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 编辑算例
    /// </summary>
    private void EditCase(SimulationCaseDto? caseDto)
    {
        if (caseDto == null) return;

        Logger.LogInformation("打开编辑算例对话框，算例：{CaseName}", caseDto.Name);
        // TODO: 打开编辑算例对话框
        MessageBox.Show($"编辑算例功能待实现\n算例：{caseDto.Name}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 删除算例
    /// </summary>
    private void DeleteCase(SimulationCaseDto? caseDto)
    {
        if (caseDto == null) return;

        var result = MessageBox.Show(
            $"确定要删除算例\"{caseDto.Name}\"吗？\n删除后将无法恢复。",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _ = ExecuteDeleteAsync(caseDto.Id);
        }
    }

    /// <summary>
    /// 执行删除
    /// </summary>
    private async Task ExecuteDeleteAsync(int id)
    {
        await ExecuteAsync(async () =>
        {
            var success = await _simulationCaseService.DeleteAsync(id);
            if (success)
            {
                MessageBox.Show("算例删除成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync(); // 刷新数据
            }
            else
            {
                MessageBox.Show("算例删除失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "删除算例中...");
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    private void BatchDelete()
    {
        if (SelectedCases.Count == 0) return;

        var result = MessageBox.Show(
            $"确定要删除选中的 {SelectedCases.Count} 个算例吗？\n删除后将无法恢复。",
            "确认批量删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var ids = SelectedCases.Select(x => x.Id).ToList();
            _ = ExecuteBatchDeleteAsync(ids);
        }
    }

    /// <summary>
    /// 执行批量删除
    /// </summary>
    private async Task ExecuteBatchDeleteAsync(List<int> ids)
    {
        await ExecuteAsync(async () =>
        {
            var deletedCount = await _simulationCaseService.BatchDeleteAsync(ids);
            MessageBox.Show($"成功删除 {deletedCount} 个算例", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            
            SelectedCases.Clear();
            await LoadDataAsync(); // 刷新数据
        }, "批量删除算例中...");
    }

    /// <summary>
    /// 查看算例详情
    /// </summary>
    private void ViewCase(SimulationCaseDto? caseDto)
    {
        if (caseDto == null) return;

        Logger.LogInformation("查看算例详情，算例：{CaseName}", caseDto.Name);
        // TODO: 打开算例详情对话框
        MessageBox.Show($"查看算例详情功能待实现\n算例：{caseDto.Name}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 打开工作路径
    /// </summary>
    private void OpenWorkingPath(SimulationCaseDto? caseDto)
    {
        if (caseDto == null) return;

        try
        {
            if (Directory.Exists(caseDto.WorkingPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = caseDto.WorkingPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show($"工作路径不存在：{caseDto.WorkingPath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "打开工作路径失败：{WorkingPath}", caseDto.WorkingPath);
            MessageBox.Show($"打开工作路径失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 上一页
    /// </summary>
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            _ = LoadDataAsync();
        }
    }

    /// <summary>
    /// 下一页
    /// </summary>
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            _ = LoadDataAsync();
        }
    }

    /// <summary>
    /// 跳转到指定页面
    /// </summary>
    private void GotoPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            _ = LoadDataAsync();
        }
    }

    #endregion
}
