using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArmorVehicleDamageAssessment.Common.Interfaces;
using ArmorVehicleDamageAssessment.Common.Models;
using ArmorVehicleDamageAssessment.Core.Models;
using ArmorVehicleDamageAssessment.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ArmorVehicleDamageAssessment.Tests.Unit.Services;

public class SimulationCaseServiceTests
{
    private readonly Mock<ISimulationCaseRepository> _repositoryMock;
    private readonly Mock<ILogger<SimulationCaseService>> _loggerMock;
    private readonly SimulationCaseService _service;

    public SimulationCaseServiceTests()
    {
        _repositoryMock = new Mock<ISimulationCaseRepository>();
        _loggerMock = new Mock<ILogger<SimulationCaseService>>();
        _service = new SimulationCaseService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCase_ReturnsDto()
    {
        // Arrange
        var caseId = 1;
        var entity = new SimulationCaseEntity
        {
            Id = caseId,
            Name = "测试算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = "/test/path",
            Status = SimulationStatus.NotCalculated,
            CreatedBy = "testuser",
            CreatedTime = DateTime.Now,
            Description = "测试描述"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(caseId))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(caseId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(caseId);
        result.Name.Should().Be("测试算例");
        result.Type.Should().Be(SimulationType.KineticPenetration);
        result.TypeDisplayName.Should().Be("动能穿甲");
        result.StatusDisplayName.Should().Be("未计算");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCase_ReturnsNull()
    {
        // Arrange
        var caseId = 999;
        _repositoryMock.Setup(x => x.GetByIdAsync(caseId))
            .ReturnsAsync((SimulationCaseEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(caseId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateSimulationCaseRequest
        {
            Name = "新建算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath(),
            Description = "测试描述",
            CreatedBy = "testuser"
        };

        var createdEntity = new SimulationCaseEntity
        {
            Id = 1,
            Name = request.Name,
            Type = request.Type,
            WorkingPath = request.WorkingPath,
            Description = request.Description,
            CreatedBy = request.CreatedBy,
            Status = SimulationStatus.NotCalculated,
            CreatedTime = DateTime.Now
        };

        _repositoryMock.Setup(x => x.ExistsAsync(request.Name, null))
            .ReturnsAsync(false);

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<SimulationCaseEntity>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(request.Name);
        result.Type.Should().Be(request.Type);
        result.Status.Should().Be(SimulationStatus.NotCalculated);
        result.CreatedBy.Should().Be(request.CreatedBy);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateSimulationCaseRequest
        {
            Name = "重复算例名称",
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath(),
            CreatedBy = "testuser"
        };

        _repositoryMock.Setup(x => x.ExistsAsync(request.Name, null))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(x => x.CreateAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("算例名称'重复算例名称'已存在");
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var request = new UpdateSimulationCaseRequest
        {
            Id = 1,
            Name = "更新后的算例",
            Type = SimulationType.ShapedCharge,
            WorkingPath = Path.GetTempPath(),
            Description = "更新后的描述"
        };

        var existingEntity = new SimulationCaseEntity
        {
            Id = request.Id,
            Name = "原始算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = "/original/path",
            Description = "原始描述",
            CreatedBy = "testuser",
            CreatedTime = DateTime.Now.AddDays(-1),
            Status = SimulationStatus.NotCalculated,
            Version = 1
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(request.Id))
            .ReturnsAsync(existingEntity);

        _repositoryMock.Setup(x => x.ExistsAsync(request.Name, request.Id))
            .ReturnsAsync(false);

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<SimulationCaseEntity>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(request);

        // Assert
        result.Should().BeTrue();

        // 验证实体是否被正确更新
        existingEntity.Name.Should().Be(request.Name);
        existingEntity.Type.Should().Be(request.Type);
        existingEntity.WorkingPath.Should().Be(request.WorkingPath);
        existingEntity.Description.Should().Be(request.Description);
        existingEntity.Version.Should().Be(2); // 版本应该递增
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCase_ReturnsFalse()
    {
        // Arrange
        var request = new UpdateSimulationCaseRequest
        {
            Id = 999,
            Name = "不存在的算例",
            Type = SimulationType.KineticPenetration,
            WorkingPath = Path.GetTempPath()
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(request.Id))
            .ReturnsAsync((SimulationCaseEntity?)null);

        // Act
        var result = await _service.UpdateAsync(request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingCase_ReturnsTrue()
    {
        // Arrange
        var caseId = 1;
        _repositoryMock.Setup(x => x.DeleteAsync(caseId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(caseId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.DeleteAsync(caseId), Times.Once);
    }

    [Fact]
    public async Task BatchDeleteAsync_ValidIds_ReturnsDeletedCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };
        var deletedCount = 3;

        _repositoryMock.Setup(x => x.BatchDeleteAsync(ids))
            .ReturnsAsync(deletedCount);

        // Act
        var result = await _service.BatchDeleteAsync(ids);

        // Assert
        result.Should().Be(deletedCount);
        _repositoryMock.Verify(x => x.BatchDeleteAsync(ids), Times.Once);
    }

    [Fact]
    public async Task BatchDeleteAsync_EmptyIds_ReturnsZero()
    {
        // Arrange
        var ids = new List<int>();

        // Act
        var result = await _service.BatchDeleteAsync(ids);

        // Assert
        result.Should().Be(0);
        _repositoryMock.Verify(x => x.BatchDeleteAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidCase_ReturnsTrue()
    {
        // Arrange
        var caseId = 1;
        var newStatus = SimulationStatus.Calculating;

        _repositoryMock.Setup(x => x.UpdateStatusAsync(caseId, newStatus))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateStatusAsync(caseId, newStatus);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.UpdateStatusAsync(caseId, newStatus), Times.Once);
    }

    [Fact]
    public async Task GetPagedListAsync_ValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var request = new SimulationCasePagedRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchKeyword = "测试"
        };

        var entities = new List<SimulationCaseEntity>
        {
            new()
            {
                Id = 1,
                Name = "测试算例1",
                Type = SimulationType.KineticPenetration,
                WorkingPath = "/test/path1",
                Status = SimulationStatus.NotCalculated,
                CreatedBy = "testuser",
                CreatedTime = DateTime.Now
            },
            new()
            {
                Id = 2,
                Name = "测试算例2",
                Type = SimulationType.ShapedCharge,
                WorkingPath = "/test/path2",
                Status = SimulationStatus.Completed,
                CreatedBy = "testuser",
                CreatedTime = DateTime.Now.AddHours(-1)
            }
        };

        var pagedResult = new PagedResult<SimulationCaseEntity>
        {
            Items = entities,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _repositoryMock.Setup(x => x.GetPagedListAsync(request))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.GetPagedListAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items[0].Name.Should().Be("测试算例1");
        result.Items[1].Name.Should().Be("测试算例2");
    }
}
