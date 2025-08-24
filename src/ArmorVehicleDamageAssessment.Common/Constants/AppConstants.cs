namespace ArmorVehicleDamageAssessment.Common.Constants;

/// <summary>
/// 应用程序常量
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// 应用程序名称
    /// </summary>
    public const string ApplicationName = "装甲车辆抗毁伤数字化评估软件平台";

    /// <summary>
    /// 应用程序版本
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// 开发机构
    /// </summary>
    public const string Organization = "沈阳理工大学";

    /// <summary>
    /// 数据库相关常量
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// 默认数据库文件名
        /// </summary>
        public const string DefaultDatabaseFileName = "ArmorVehicleAssessment.db";

        /// <summary>
        /// 连接字符串键名
        /// </summary>
        public const string ConnectionStringKey = "DefaultConnection";
    }

    /// <summary>
    /// UI相关常量
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// 默认页面大小
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// 最大页面大小
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>
        /// 搜索延迟（毫秒）
        /// </summary>
        public const int SearchDelayMs = 500;
    }

    /// <summary>
    /// 文件和目录相关常量
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// 默认工作目录名称
        /// </summary>
        public const string DefaultWorkspaceName = "ArmorVehicleWorkspace";

        /// <summary>
        /// 配置文件名称
        /// </summary>
        public const string ConfigFileName = "appsettings.json";

        /// <summary>
        /// 日志目录名称
        /// </summary>
        public const string LogDirectoryName = "Logs";
    }

    /// <summary>
    /// 消息类型常量
    /// </summary>
    public static class MessageType
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Success = "Success";
    }
}
