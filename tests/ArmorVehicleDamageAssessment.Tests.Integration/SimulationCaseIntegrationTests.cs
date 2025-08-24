using System;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Interfaces;
using ArmorVehicleDamageAssessment.Core.Interfaces;
using ArmorVehicleDamageAssessment.Core.Models;
using ArmorVehicleDamageAssessment.Core.Services;
using ArmorVehicleDamageAssessment.Data.Context;
using ArmorVehicleDamageAssessment.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ArmorVehicleDamageAssessment.Tests.Integration;

public class SimulationCaseIntegrationTests : IDisposable
{
    private readonly ArmorVehicleDbContext _context;
    private readonly ISimulationCaseRepository _repository;
    private readonly ISimulationCaseService _service;
    private readonly ITestOutputHelper _output;

    public SimulationCaseIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // 创建内存数据库
        var options = new DbContextOptionsBuilder<ArmorVehicleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ArmorVehicleDbContext(options);

        // 确保数据库创建
        _context.Database.EnsureCreated();

        // 创建日志记录器
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var repositoryLogger = loggerFactory.CreateLogger<SimulationCaseRepository>();
        var serviceLogger = loggerFactory.CreateLogger<SimulationCaseService>();

        // 创建服务实例
        _repository = new SimulationCaseRepository(_context, repositoryLogger);
        _service = new SimulationCaseService(_repository, serviceLogger);
    }

    [Fact]
    public async Task CreateReadUpdateDelete_FullWorkflow_Success()
    {
        _output.WriteLine("开始执行CRUD集成测试");

        // 1. Create - 创建算例
        var createRequest = new CreateSimulationCaseRequest
        {
            Name = "集成测试算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath(),
            Description = "这是一个集成测试算例",
            CreatedBy = "integration_test"
        };

        _output.WriteLine($"创建算例: {createRequest.Name}");
        var createdCase = await _service.CreateAsync(createRequest);
        
        createdCase.Should().NotBeNull();
        createdCase.Id.Should().BeGreaterThan(0);
        createdCase.Name.Should().Be(createRequest.Name);
        createdCase.Status.Should().Be(SimulationStatus.NotCalculated);
        _output.WriteLine($"算例创建成功，ID: {createdCase.Id}");

        // 2. Read - 读取算例
        _output.WriteLine($"读取算例，ID: {createdCase.Id}");
        var retrievedCase = await _service.GetByIdAsync(createdCase.Id);
        
        retrievedCase.Should().NotBeNull();
        retrievedCase!.Id.Should().Be(createdCase.Id);
        retrievedCase.Name.Should().Be(createdCase.Name);
        _output.WriteLine("算例读取成功");

        // 3. Update - 更新算例
        var updateRequest = new UpdateSimulationCaseRequest
        {
            Id = createdCase.Id,
            Name = "更新后的集成测试算例",
            Type = SimulationType.ShapedCharge,
            WorkingPath = Path.GetTempPath(),
            Description = "这是更新后的描述"
        };

        _output.WriteLine($"更新算例，新名称: {updateRequest.Name}");
        var updateResult = await _service.UpdateAsync(updateRequest);
        
        updateResult.Should().BeTrue();
        _output.WriteLine("算例更新成功");

        // 验证更新结果
        var updatedCase = await _service.GetByIdAsync(createdCase.Id);
        updatedCase.Should().NotBeNull();
        updatedCase!.Name.Should().Be(updateRequest.Name);
        updatedCase.Type.Should().Be(updateRequest.Type);
        updatedCase.Description.Should().Be(updateRequest.Description);

        // 4. Delete - 删除算例
        _output.WriteLine($"删除算例，ID: {createdCase.Id}");
        var deleteResult = await _service.DeleteAsync(createdCase.Id);
        
        deleteResult.Should().BeTrue();
        _output.WriteLine("算例删除成功");

        // 验证删除结果
        var deletedCase = await _service.GetByIdAsync(createdCase.Id);
        deletedCase.Should().BeNull();
        _output.WriteLine("CRUD集成测试完成");
    }

    [Fact]
    public async Task PagedQuery_WithFilters_ReturnsCorrectResults()
    {
        _output.WriteLine("开始分页查询集成测试");

        // 创建测试数据
        var testCases = new[]
        {
            new CreateSimulationCaseRequest
            {
                Name = "动能穿甲算例1",
                Type = SimulationType.KineticPenetration,
                WorkingPath = Path.GetTempPath(),
                CreatedBy = "user1"
            },
            new CreateSimulationCaseRequest
            {
                Name = "聚能破甲算例1",
                Type = SimulationType.ShapedCharge,
                WorkingPath = Path.GetTempPath(),
                CreatedBy = "user2"
            },
            new CreateSimulationCaseRequest
            {
                Name = "动能穿甲算例2",
                Type = SimulationType.KineticPenetration,
                WorkingPath = Path.GetTempPath(),
                CreatedBy = "user1"
            }
        };

        var createdCases = new List<SimulationCaseDto>();
        foreach (var request in testCases)
        {
            var created = await _service.CreateAsync(request);
            createdCases.Add(created);
            _output.WriteLine($"创建测试数据: {created.Name}");
        }

        // 1. 测试基本分页查询
        var pagedRequest = new SimulationCasePagedRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = await _service.GetPagedListAsync(pagedRequest);
        
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(3);
        pagedResult.TotalCount.Should().Be(3);
        _output.WriteLine($"基本分页查询成功，返回 {pagedResult.Items.Count} 项");

        // 2. 测试类型筛选
        var filteredRequest = new SimulationCasePagedRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Type = SimulationType.KineticPenetration
        };

        var filteredResult = await _service.GetPagedListAsync(filteredRequest);
        
        filteredResult.Should().NotBeNull();
        filteredResult.Items.Should().HaveCount(2);
        filteredResult.Items.Should().AllSatisfy(x => x.Type.Should().Be(SimulationType.KineticPenetration));
        _output.WriteLine($"类型筛选查询成功，返回 {filteredResult.Items.Count} 项");

        // 3. 测试搜索关键词
        var searchRequest = new SimulationCasePagedRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchKeyword = "聚能"
        };

        var searchResult = await _service.GetPagedListAsync(searchRequest);
        
        searchResult.Should().NotBeNull();
        searchResult.Items.Should().HaveCount(1);
        searchResult.Items[0].Name.Should().Contain("聚能");
        _output.WriteLine($"关键词搜索成功，返回 {searchResult.Items.Count} 项");

        _output.WriteLine("分页查询集成测试完成");
    }

    [Fact]
    public async Task BatchOperations_MultipleItems_Success()
    {
        _output.WriteLine("开始批量操作集成测试");

        // 创建多个测试算例
        var createRequests = Enumerable.Range(1, 5).Select(i =>
            new CreateSimulationCaseRequest
            {
                Name = $"批量测试算例{i}",
                Type = SimulationType.KineticPenetration,
                WorkingPath = Path.GetTempPath(),
                CreatedBy = "batch_test"
            }).ToArray();

        var createdCases = new List<SimulationCaseDto>();
        foreach (var request in createRequests)
        {
            var created = await _service.CreateAsync(request);
            createdCases.Add(created);
        }

        _output.WriteLine($"创建了 {createdCases.Count} 个测试算例");

        // 测试批量删除
        var idsToDelete = createdCases.Take(3).Select(x => x.Id).ToList();
        var deletedCount = await _service.BatchDeleteAsync(idsToDelete);

        deletedCount.Should().Be(3);
        _output.WriteLine($"批量删除成功，删除了 {deletedCount} 个算例");

        // 验证删除结果
        var remainingResult = await _service.GetPagedListAsync(new SimulationCasePagedRequest
        {
            PageNumber = 1,
            PageSize = 10,
            CreatedBy = "batch_test"
        });

        remainingResult.TotalCount.Should().Be(2);
        _output.WriteLine($"验证批量删除成功，剩余 {remainingResult.TotalCount} 个算例");
        _output.WriteLine("批量操作集成测试完成");
    }

    [Fact]
    public async Task StatusUpdate_ValidCase_UpdatesSuccessfully()
    {
        _output.WriteLine("开始状态更新集成测试");

        // 创建测试算例
        var createRequest = new CreateSimulationCaseRequest
        {
            Name = "状态测试算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath(),
            CreatedBy = "status_test"
        };

        var createdCase = await _service.CreateAsync(createRequest);
        createdCase.Status.Should().Be(SimulationStatus.NotCalculated);
        _output.WriteLine($"创建算例，初始状态: {createdCase.StatusDisplayName}");

        // 更新状态为计算中
        var updateResult = await _service.UpdateStatusAsync(createdCase.Id, SimulationStatus.Calculating);
        updateResult.Should().BeTrue();
        _output.WriteLine("状态更新为计算中");

        // 验证状态更新
        var updatedCase = await _service.GetByIdAsync(createdCase.Id);
        updatedCase.Should().NotBeNull();
        updatedCase!.Status.Should().Be(SimulationStatus.Calculating);
        _output.WriteLine($"验证状态更新成功: {updatedCase.StatusDisplayName}");

        // 更新状态为计算完成
        await _service.UpdateStatusAsync(createdCase.Id, SimulationStatus.Completed);
        var completedCase = await _service.GetByIdAsync(createdCase.Id);
        completedCase!.Status.Should().Be(SimulationStatus.Completed);
        _output.WriteLine($"最终状态: {completedCase.StatusDisplayName}");

        _output.WriteLine("状态更新集成测试完成");
    }

    [Fact]
    public async Task DuplicateNameValidation_PreventsDuplicates()
    {
        _output.WriteLine("开始重复名称验证集成测试");

        var duplicateName = "重复名称测试算例";

        // 创建第一个算例
        var firstRequest = new CreateSimulationCaseRequest
        {
            Name = duplicateName,
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath(),
            CreatedBy = "duplicate_test"
        };

        var firstCase = await _service.CreateAsync(firstRequest);
        firstCase.Should().NotBeNull();
        _output.WriteLine($"第一个算例创建成功: {firstCase.Name}");

        // 尝试创建同名算例
        var duplicateRequest = new CreateSimulationCaseRequest
        {
            Name = duplicateName,
            Type = SimulationType.ShapedCharge,
            WorkingPath = Path.GetTempPath(),
            CreatedBy = "duplicate_test"
        };

        await _service.Invoking(s => s.CreateAsync(duplicateRequest))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"算例名称'{duplicateName}'已存在");

        _output.WriteLine("重复名称验证成功，正确阻止了重复创建");

        // 验证更新时的重复名称检查
        var anotherRequest = new CreateSimulationCaseRequest
        {
            Name = "另一个算例",
            Type = SimulationType.ExplosiveImpact,
            WorkingPath = Path.GetTempPath(),
            CreatedBy = "duplicate_test"
        };

        var anotherCase = await _service.CreateAsync(anotherRequest);

        // 尝试将另一个算例更新为重复名称
        var updateRequest = new UpdateSimulationCaseRequest
        {
            Id = anotherCase.Id,
            Name = duplicateName,
            Type = SimulationType.ExplosiveImpact,
            WorkingPath = Path.GetTempPath()
        };

        await _service.Invoking(s => s.UpdateAsync(updateRequest))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"算例名称'{duplicateName}'已被其他算例使用");

        _output.WriteLine("更新时重复名称验证成功");
        _output.WriteLine("重复名称验证集成测试完成");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
