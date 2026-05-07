using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class TaskTemplateServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private TaskTemplateService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new TaskTemplateService(new TaskTemplateRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.TaskTemplates.AddRange(
            new TaskTemplate
            {
                TaskTemplateId = 4801,
                TaskName = "Prepare hall",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new TaskTemplate
            {
                TaskTemplateId = 4802,
                TaskName = "Serve dishes",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskTemplate
            {
                TaskTemplateId = 4803,
                TaskName = "Clean area",
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            });

        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 90001,
            TaskTemplateId = 4802,
            TaskName = "Serve dishes",
            TaskStatus = "PENDING"
        });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 47 - GetAllTaskTemplatesFiltered
    //Function 47 - TC1
    [TestMethod]
    public async Task GetTaskTemplatesAsync_WhenFilterByIsActive_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetTaskTemplatesAsync(
            new TaskTemplateFilterRequest { IsActive = true },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.IsActive == true);
    }

    //Function 47 - TC2
    [TestMethod]
    public async Task GetTaskTemplatesAsync_WhenPaged_ShouldReturnCorrectPage()
    {
        var result = await _sut.GetTaskTemplatesAsync(
            new TaskTemplateFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().TaskTemplateId.Should().Be(4802);
    }
    #endregion

    #region Function 48 - CreateTaskTemplate
    //Function 48 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenTaskNameMissing_ShouldFail()
    {
        var result = await _sut.CreateAsync(new TaskTemplateCreateRequest
        {
            TaskName = "   ",
            IsActive = true
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("TaskName is required.");
        result.Data.Should().BeNull();
    }

    //Function 48 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenTaskNameDuplicatedIgnoringCase_ShouldFail()
    {
        var result = await _sut.CreateAsync(new TaskTemplateCreateRequest
        {
            TaskName = "prepare HALL",
            IsActive = true
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("TaskName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 48 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenValidAndIsActiveNull_ShouldCreateWithDefaultActiveTrue()
    {
        var result = await _sut.CreateAsync(new TaskTemplateCreateRequest
        {
            TaskName = "  Setup stage  ",
            IsActive = null
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Task template created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Setup stage");
        result.Data.IsActive.Should().BeTrue();

        var saved = await _dbContext.TaskTemplates.AsNoTracking()
            .FirstAsync(x => x.TaskTemplateId == result.Data.TaskTemplateId);
        saved.TaskName.Should().Be("Setup stage");
        saved.IsActive.Should().BeTrue();
    }
    #endregion

    #region Function 49 - UpdateTaskTemplate
    //Function 49 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenTaskTemplateNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new TaskTemplateUpdateRequest
        {
            TaskName = "New Name",
            IsActive = true
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task template not found.");
        result.Data.Should().BeNull();
    }

    //Function 49 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenTaskNameMissing_ShouldFail()
    {
        var result = await _sut.UpdateAsync(4801, new TaskTemplateUpdateRequest
        {
            TaskName = " ",
            IsActive = false
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("TaskName is required.");
        result.Data.Should().BeNull();
    }

    //Function 49 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenTaskNameDuplicatedIgnoringCase_ShouldFail()
    {
        var result = await _sut.UpdateAsync(4801, new TaskTemplateUpdateRequest
        {
            TaskName = "serve DISHES",
            IsActive = true
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("TaskName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 49 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var before = await _dbContext.TaskTemplates.AsNoTracking().FirstAsync(x => x.TaskTemplateId == 4801);

        var result = await _sut.UpdateAsync(4801, new TaskTemplateUpdateRequest
        {
            TaskName = "  Prepare hall v2  ",
            IsActive = false
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Task template updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Prepare hall v2");
        result.Data.IsActive.Should().BeFalse();

        var saved = await _dbContext.TaskTemplates.AsNoTracking().FirstAsync(x => x.TaskTemplateId == 4801);
        saved.TaskName.Should().Be("Prepare hall v2");
        saved.IsActive.Should().BeFalse();
        saved.UpdatedAt.Should().BeAfter(before.UpdatedAt ?? DateTime.MinValue);
    }
    #endregion

    #region Function 50 - DeleteTaskTemplate
    //Function 50 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenTaskTemplateNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Task template not found.");
        result.Data.Should().BeFalse();
    }

    //Function 50 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenTemplateInUse_ShouldDeactivateInsteadOfDelete()
    {
        var result = await _sut.DeleteAsync(4802);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Task template is in use and has been deactivated instead of deleted.");
        result.Data.Should().BeTrue();

        var saved = await _dbContext.TaskTemplates.AsNoTracking().FirstAsync(x => x.TaskTemplateId == 4802);
        saved.IsActive.Should().BeFalse();
    }

    //Function 50 - TC3
    [TestMethod]
    public async Task DeleteAsync_WhenTemplateNotInUse_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(4803);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Task template deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.TaskTemplates.AnyAsync(x => x.TaskTemplateId == 4803)).Should().BeFalse();
    }
    #endregion
}

