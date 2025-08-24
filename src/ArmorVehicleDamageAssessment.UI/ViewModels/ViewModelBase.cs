using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace ArmorVehicleDamageAssessment.UI.ViewModels;

/// <summary>
/// ViewModel基类
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    protected readonly ILogger Logger;

    protected ViewModelBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private bool _isBusy;
    /// <summary>
    /// 是否忙碌状态
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _busyMessage = string.Empty;
    /// <summary>
    /// 忙碌状态消息
    /// </summary>
    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    private string _errorMessage = string.Empty;
    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _hasError;
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    /// <summary>
    /// 设置忙碌状态
    /// </summary>
    /// <param name="message">忙碌消息</param>
    protected void SetBusy(string message = "处理中...")
    {
        IsBusy = true;
        BusyMessage = message;
        ClearError();
    }

    /// <summary>
    /// 清除忙碌状态
    /// </summary>
    protected void ClearBusy()
    {
        IsBusy = false;
        BusyMessage = string.Empty;
    }

    /// <summary>
    /// 设置错误状态
    /// </summary>
    /// <param name="message">错误消息</param>
    protected void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        ClearBusy();
        Logger.LogError("ViewModel错误: {Message}", message);
    }

    /// <summary>
    /// 清除错误状态
    /// </summary>
    protected void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// 安全执行异步操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="busyMessage">忙碌消息</param>
    /// <param name="onError">错误处理</param>
    protected async Task ExecuteAsync(Func<Task> action, string busyMessage = "处理中...", Action<Exception>? onError = null)
    {
        try
        {
            SetBusy(busyMessage);
            await action();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            SetError($"操作失败: {ex.Message}");
        }
        finally
        {
            ClearBusy();
        }
    }

    /// <summary>
    /// 安全执行异步操作（有返回值）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="func">要执行的函数</param>
    /// <param name="busyMessage">忙碌消息</param>
    /// <param name="onError">错误处理</param>
    /// <returns>操作结果</returns>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> func, string busyMessage = "处理中...", Func<Exception, T>? onError = null)
    {
        try
        {
            SetBusy(busyMessage);
            return await func();
        }
        catch (Exception ex)
        {
            if (onError != null)
            {
                return onError(ex);
            }

            SetError($"操作失败: {ex.Message}");
            return default;
        }
        finally
        {
            ClearBusy();
        }
    }
}
