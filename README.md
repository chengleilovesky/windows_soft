# 装甲车辆抗毁伤数字化评估软件平台

> 沈阳理工大学开发的专业仿真评估系统 v1.0.0

## 🎯 项目概述

装甲车辆抗毁伤数字化评估软件平台是基于 .NET 8.0 和 WPF 技术开发的专业军工仿真软件，用于装甲车辆的毁伤效果数字化建模、仿真计算和评估分析。

### 核心功能
- 🎯 **动能穿甲仿真** - 高精度弹道计算和穿甲效果评估
- ⚡ **聚能破甲仿真** - 聚能战斗部破甲机理建模
- 💥 **爆炸冲击仿真** - 爆炸载荷传播和结构响应分析
- 📊 **算例管理** - 仿真方案创建、执行和结果管理
- 🗄️ **数据库管理** - 材料库、车辆模型库、制式弹模型库

### 技术特色
- **现代化UI**: Material Design设计风格，深色主题
- **模块化架构**: MVVM模式，分层设计，易于扩展
- **高性能**: 异步编程，多线程计算
- **数据持久化**: SQLite数据库，Entity Framework Core
- **专业日志**: NLog日志系统，便于调试和维护

## 🚀 快速开始

### 📦 直接运行发布版本

最简单的方式是直接运行打包好的程序：

```bash
1. 解压 publish/ArmorVehicleDamageAssessment-Windows-v1.0.0.zip
2. 双击 ArmorVehicleDamageAssessment.UI.exe
3. 程序启动，显示图形界面
```

### 🛠️ 使用便捷脚本

我们提供了智能启动脚本：

```powershell
# PowerShell脚本 (推荐)
cd scripts
.\run-app.ps1

# Windows批处理
.\run-app.bat
```

### 📋 详细指南

- 📖 [完整运行说明](docs/运行说明.md) - 详细的安装和运行指导
- ⚡ [快速启动指南](docs/快速启动指南.md) - 一分钟快速上手
- 🔧 [构建脚本说明](scripts/build-and-publish.ps1) - 自动化构建流程

## 🏗️ 项目结构

```
ArmorVehicleDamageAssessment/
├── src/                               # 源代码
│   ├── ArmorVehicleDamageAssessment.UI/        # WPF界面层
│   ├── ArmorVehicleDamageAssessment.Core/      # 核心业务逻辑
│   ├── ArmorVehicleDamageAssessment.Data/      # 数据访问层
│   └── ArmorVehicleDamageAssessment.Common/    # 公共组件
├── tests/                             # 测试项目
│   ├── ArmorVehicleDamageAssessment.Tests.Unit/      # 单元测试
│   └── ArmorVehicleDamageAssessment.Tests.Integration/ # 集成测试
├── docs/                              # 文档
│   ├── 运行说明.md                     # 详细运行指南
│   ├── 快速启动指南.md                 # 快速参考
│   └── 装甲车辆抗毁伤数字化评估软件平台-PRD.md # 产品需求文档
├── scripts/                           # 脚本工具
│   ├── build-and-publish.ps1          # 构建发布脚本
│   ├── run-app.ps1                   # PowerShell启动脚本
│   └── run-app.bat                   # 批处理启动脚本
├── publish/                           # 发布文件
│   ├── win-x64-self-contained/        # 自包含版本
│   ├── win-x64-framework-dependent/   # 框架依赖版本
│   └── ArmorVehicleDamageAssessment-Windows-v1.0.0.zip # 发布包
└── ArmorVehicleDamageAssessment.sln   # 解决方案文件
```

## 💻 技术栈

### 核心技术
- **.NET 8.0** - 现代化的跨平台开发框架
- **WPF (Windows Presentation Foundation)** - 丰富的桌面应用UI框架
- **Material Design in XAML** - Google Material Design设计语言
- **Entity Framework Core** - 现代化ORM框架
- **SQLite** - 轻量级嵌入式数据库

### 开发工具和库
- **CommunityToolkit.Mvvm** - MVVM模式支持
- **NLog** - 专业日志框架
- **Microsoft.Extensions.*** - 依赖注入、配置管理
- **xUnit** - 单元测试框架
- **FluentAssertions** - 测试断言库

## 🎨 界面预览

### 主界面特色
- **标题栏**: 软件名称、版本信息、快捷工具
- **导航栏**: 功能模块导航，图标化设计
- **内容区**: 动态内容展示，支持多标签页
- **状态栏**: 系统状态、版权信息

### 主要功能模块
1. **算例管理** - 仿真方案的CRUD操作
2. **仿真计算** - 三种仿真类型的专业计算
3. **数据管理** - 材料、车辆、弹药数据库
4. **结果分析** - 可视化结果展示

## 🔧 系统要求

### 最低配置
- **操作系统**: Windows 10 版本 1809 (64位)
- **处理器**: Intel Core i3 或 AMD 等效处理器
- **内存**: 4 GB RAM
- **存储**: 500 MB 可用磁盘空间
- **显卡**: 支持 DirectX 11

### 推荐配置
- **操作系统**: Windows 11 (64位)
- **处理器**: Intel Core i5 或 AMD Ryzen 5
- **内存**: 8 GB RAM
- **存储**: 1 GB 可用磁盘空间 (SSD)
- **显卡**: 独立显卡，支持硬件加速

### 软件依赖
- **.NET 8.0 运行时** (框架依赖版本需要)
- **Visual C++ Redistributable** (通常系统已包含)

## 📦 发布版本说明

### 自包含版本 (推荐)
- **文件大小**: ~100-200MB
- **优点**: 无需安装.NET运行时，可在任何Windows机器运行
- **适用**: 生产环境、客户部署

### 框架依赖版本
- **文件大小**: ~50MB
- **优点**: 体积小，启动快
- **前提**: 需要预装.NET 8.0运行时
- **适用**: 开发环境、已有.NET环境的机器

## 🛠️ 开发指南

### 构建项目
```bash
# 克隆仓库
git clone <repository-url>
cd ArmorVehicleDamageAssessment

# 还原依赖
dotnet restore

# 编译项目
dotnet build --configuration Release

# 运行程序
dotnet run --project src/ArmorVehicleDamageAssessment.UI
```

### 发布打包
```powershell
# 使用自动化脚本
.\scripts\build-and-publish.ps1

# 手动发布自包含版本
dotnet publish src/ArmorVehicleDamageAssessment.UI/ArmorVehicleDamageAssessment.UI.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output publish/win-x64-self-contained
```

### 运行测试
```bash
# 运行所有测试
dotnet test

# 运行单元测试
dotnet test tests/ArmorVehicleDamageAssessment.Tests.Unit

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 文档资源

| 文档 | 描述 | 位置 |
|------|------|------|
| 运行说明 | 详细的安装运行指导 | [docs/运行说明.md](docs/运行说明.md) |
| 快速指南 | 一分钟上手指南 | [docs/快速启动指南.md](docs/快速启动指南.md) |
| 产品需求 | 项目需求和规格说明 | [docs/装甲车辆抗毁伤数字化评估软件平台-PRD.md](docs/装甲车辆抗毁伤数字化评估软件平台-PRD.md) |
| API文档 | 代码注释生成的API文档 | 构建后生成 |

## 🚨 故障排除

### 常见问题
1. **程序无法启动** → 检查.NET运行时、杀毒软件拦截
2. **界面显示异常** → 调整系统DPI设置、更新显卡驱动
3. **数据库错误** → 检查文件权限、磁盘空间
4. **计算结果异常** → 查看日志文件、验证输入参数

### 日志位置
- **程序日志**: `logs/yyyy-MM-dd.log`
- **系统事件**: Windows事件查看器 → 应用程序日志
- **错误报告**: 程序异常时自动生成

### 技术支持
- **开发单位**: 沈阳理工大学
- **项目版本**: 1.0.0
- **更新日期**: 2025年1月

## 📄 许可证

本项目为沈阳理工大学内部项目，版权所有。

## 🔗 相关链接

- [.NET 8.0 下载](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Material Design 指南](https://material.io/design)
- [WPF 官方文档](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

*本项目由沈阳理工大学开发团队精心打造，致力于为装甲车辆防护研究提供专业的数字化评估工具。*
