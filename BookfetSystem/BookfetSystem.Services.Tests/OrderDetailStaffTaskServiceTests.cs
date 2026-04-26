using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;
using System.Threading;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class OrderDetailStaffTaskServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private OrderDetailStaffTaskService _sut = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<IStaffTaskOverdueSchedulerService> _schedulerMock = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock.Setup(x => x.SendToUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        _schedulerMock = new Mock<IStaffTaskOverdueSchedulerService>();
        _schedulerMock.Setup(x => x.ScheduleTaskOverdueCheckAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CloudinaryFolder>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://res.cloudinary.com/demo/image/upload/task/evidence.jpg");

        _sut = new OrderDetailStaffTaskService(
            new OrderDetailStaffTaskRepository(_dbContext),
            new OrderDetailRepository(_dbContext),
            new UserRepository(_dbContext),
            new TaskTemplateRepository(_dbContext),
            new StaffGroupRepository(_dbContext),
            new StaffGroupMemberRepository(_dbContext),
            _notificationServiceMock.Object,
            _schedulerMock.Object,
            _imageStorageServiceMock.Object);

        await SeedTaskWorkflowDataAsync();
    }

    #region Seed Data
    private async Task SeedTaskWorkflowDataAsync()
    {
        _dbContext.Users.AddRange(
            new User
            {
                UserId = 1,
                FullName = "Customer One",
                Email = "customer@test.com",
                Phone = "0900000001",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "customer1",
                Address = "HN",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                FullName = "Leader Two",
                Email = "leader@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "leader2",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 3,
                FullName = "Staff Three",
                Email = "staff@test.com",
                Phone = "0900000003",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "staff3",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 4,
                FullName = "Outsider Four",
                Email = "out@test.com",
                Phone = "0900000004",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "out4",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.MenuCategories.Add(new MenuCategory
        {
            MenuCategoryId = 1,
            MenuCategoryName = "Set",
            Description = "desc",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Menus.Add(new Menu
        {
            MenuId = 1,
            MenuName = "Standard Menu",
            BasePrice = 200_000,
            Status = "AVAILABLE",
            ImgUrl = "[]",
            MenuCategoryId = 1,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.PartyCategories.Add(new PartyCategory
        {
            PartyCategoryId = 1,
            PartyCategoryName = "Wedding",
            Description = "desc",
            Status = "AVAILABLE",
            NumberOfGuests = 10,
            ImageUrl = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.StaffGroups.Add(new StaffGroup
        {
            StaffGroupId = 10,
            StaffGroupName = "Service Team",
            Status = StaffGroupStatus.ACTIVE.ToString(),
            LeaderId = 2
        });

        _dbContext.StaffGroupMembers.Add(new StaffGroupMember
        {
            StaffGroupMemberId = 1,
            StaffGroupId = 10,
            StaffId = 3,
            Status = "ACTIVE"
        });

        _dbContext.TaskTemplates.Add(new TaskTemplate
        {
            TaskTemplateId = 1,
            TaskName = "Chuẩn bị sảnh",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        var partyStart = DateTime.UtcNow.AddDays(14);
        _dbContext.Orders.Add(new Order
        {
            OrderId = 900,
            CustomerId = 1,
            Status = OrderStatus.APPROVED.ToString(),
            TotalPrice = 5_000_000,
            DepositAmount = 1_000_000,
            RemainingAmount = 0,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 9001,
            OrderId = 900,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 50,
            Address = "HN",
            Status = OrderDetailStatus.PREPARING.ToString(),
            StaffGroupId = 10,
            StartTime = partyStart,
            EndTime = partyStart.AddHours(4)
        });

        await _dbContext.SaveChangesAsync();
    }

    private async Task<OrderDetailStaffTask> SeedTaskAsync(
        int taskId,
        int staffId,
        StaffTaskStatus status,
        DateTime? endTime,
        string? taskName = null,
        int? orderDetailId = 9001)
    {
        var entity = new OrderDetailStaffTask
        {
            TaskId = taskId,
            OrderDetailId = orderDetailId,
            TaskTemplateId = 1,
            StaffId = staffId,
            TaskName = taskName ?? $"Task {taskId}",
            TaskStatus = status.ToString(),
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = endTime
        };

        _dbContext.OrderDetailStaffTasks.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }
    #endregion

    #region Function 90 - Create Task Validation
    //Function 90 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenLeaderHasNoActiveStaffGroup_ShouldFail()
    {
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 4);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Trưởng nhóm chưa có nhóm nhân sự đang hoạt động.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailNotFound_ShouldFail()
    {
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 99999,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy chi tiết đơn tiệc.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailNotInLeadersGroup_ShouldFail()
    {
        var otherDetail = new OrderDetail
        {
            OrderDetailId = 9101,
            OrderId = 900,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 10,
            Address = "HN",
            Status = OrderDetailStatus.APPROVED.ToString(),
            StaffGroupId = 99,
            StartTime = DateTime.UtcNow.AddDays(20),
            EndTime = DateTime.UtcNow.AddDays(20).AddHours(2)
        };
        _dbContext.OrderDetails.Add(otherDetail);
        await _dbContext.SaveChangesAsync();

        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9101,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Chi tiết đơn tiệc không thuộc nhóm của bạn.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenStaffNotFound_ShouldFail()
    {
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 9999,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy nhân viên.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenStaffNotInLeadersGroup_ShouldFail()
    {
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 4,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Nhân viên không thuộc nhóm của bạn.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC6
    [TestMethod]
    public async Task CreateAsync_WhenNoActiveTaskTemplate_ShouldStillCreateWithoutTemplate()
    {
        var templates = await _dbContext.TaskTemplates.ToListAsync();
        _dbContext.TaskTemplates.RemoveRange(templates);
        await _dbContext.SaveChangesAsync();

        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Công việc lạ",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Tạo công việc thành công.");
        result.Data.Should().NotBeNull();

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking()
            .FirstAsync(t => t.TaskId == result.Data!.TaskId);
        saved.TaskTemplateId.Should().BeNull();
        saved.TaskName.Should().Be("Công việc lạ");
    }

    //Function 90 - TC7
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailHasNoStaffGroup_ShouldFail()
    {
        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 9102,
            OrderId = 900,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 10,
            Address = "HN",
            Status = OrderDetailStatus.APPROVED.ToString(),
            StaffGroupId = null,
            StartTime = DateTime.UtcNow.AddDays(20),
            EndTime = DateTime.UtcNow.AddDays(20).AddHours(2)
        });
        await _dbContext.SaveChangesAsync();

        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9102,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Chi tiết đơn tiệc không thuộc nhóm của bạn.");
        result.Data.Should().BeNull();
    }

    //Function 90 - TC8
    [TestMethod]
    public async Task CreateAsync_WhenNoTaskNameAndNoDefaultActiveTemplate_ShouldCreateWithoutTemplate()
    {
        var template = await _dbContext.TaskTemplates.FirstAsync(x => x.TaskTemplateId == 1);
        template.IsActive = false;
        await _dbContext.SaveChangesAsync();

        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "   ",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Tạo công việc thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Công việc");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking()
            .FirstAsync(t => t.TaskId == result.Data.TaskId);
        saved.TaskTemplateId.Should().BeNull();
    }
    #endregion

    #region Function 91 - Assign Task to Staff
    //Function 91 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldAssignToStaffNotifyAndScheduleOverdueCheck()
    {
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(2);
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Chuẩn bị sảnh",
            StaffId = 3,
            StartTime = start,
            EndTime = end,
            Note = "  Ưu tiên  "
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Tạo công việc thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.StaffId.Should().Be(3);
        result.Data.TaskName.Should().Be("Chuẩn bị sảnh");
        result.Data.StaffName.Should().Be("Staff Three");
        result.Data.Note.Should().Be("Ưu tiên");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking()
            .FirstAsync(t => t.OrderDetailId == 9001 && t.StaffId == 3);
        saved.TaskStatus.Should().Be(StaffTaskStatus.PENDING.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                3,
                "Bạn có công việc mới",
                It.Is<string>(b => b.Contains("Chuẩn bị sảnh")),
                NotificationType.Task,
                It.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey("taskId") && d["orderDetailId"] == "9001")),
            Times.Once());

        _schedulerMock.Verify(x => x.ScheduleTaskOverdueCheckAsync(saved.TaskId, end), Times.Once());
    }

    //Function 91 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenTaskNameIsBlank_ShouldFallbackToCongViecAndStillCreate()
    {
        var end = DateTime.UtcNow.AddDays(3);
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "   ",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = end,
            Note = "  "
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Tạo công việc thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Công việc");
        result.Data.Note.Should().Be(string.Empty);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking()
            .FirstAsync(t => t.TaskId == result.Data.TaskId);
        saved.TaskName.Should().Be("Công việc");
        saved.TaskTemplateId.Should().Be(1);
        saved.Note.Should().Be(string.Empty);

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                3,
                "Bạn có công việc mới",
                It.Is<string>(b => b.Contains("Công việc")),
                NotificationType.Task,
                It.Is<Dictionary<string, string>>(d =>
                    d["taskId"] == saved.TaskId.ToString() && d["orderDetailId"] == "9001")),
            Times.Once());
        _schedulerMock.Verify(x => x.ScheduleTaskOverdueCheckAsync(saved.TaskId, end), Times.Once());
    }

    //Function 91 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenTaskNameNotInTemplate_ShouldUseDefaultTemplateAndKeepTaskName()
    {
        var request = new OrderDetailStaffTaskCreateRequest
        {
            OrderDetailId = 9001,
            TaskName = "Task không có template",
            StaffId = 3,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(2)
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Task không có template");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking()
            .FirstAsync(t => t.TaskId == result.Data.TaskId);
        saved.TaskTemplateId.Should().Be(1);
        saved.TaskName.Should().Be("Task không có template");
    }
    #endregion

    #region Function 92 - Update Task
    //Function 92 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenTaskNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99_999, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 3,
            TaskName = "Đổi tên"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy công việc.");
        result.Data.Should().BeNull();
    }

    //Function 92 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenOrderDetailNotFound_ShouldFail()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7101,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "T1",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7101, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 88888,
            StaffId = 3,
            TaskName = "Đổi tên"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy chi tiết đơn tiệc.");
        result.Data.Should().BeNull();
    }

    //Function 92 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenStaffNotFound_ShouldFail()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7102,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "T2",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7102, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 88888,
            TaskName = "Đổi tên"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy nhân viên.");
        result.Data.Should().BeNull();
    }

    //Function 92 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldPersistAndRescheduleOverdueCheck()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7103,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "T3",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var newEnd = DateTime.UtcNow.AddDays(5);
        var result = await _sut.UpdateAsync(7103, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 3,
            TaskName = "Chuẩn bị sảnh",
            TaskStatus = StaffTaskStatus.IN_PROGRESS,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = newEnd,
            Note = "updated"
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Cập nhật công việc thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Chuẩn bị sảnh");
        result.Data.TaskStatus.Should().Be((int)StaffTaskStatus.IN_PROGRESS);
        result.Data.Note.Should().Be("updated");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(t => t.TaskId == 7103);
        saved.TaskStatus.Should().Be(StaffTaskStatus.IN_PROGRESS.ToString());

        _schedulerMock.Verify(x => x.ScheduleTaskOverdueCheckAsync(7103, newEnd), Times.Once());
    }

    //Function 92 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenTaskNameNotMatchTemplate_ShouldKeepOldTemplateId()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7104,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "Old Name",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7104, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 3,
            TaskName = "Tên lạ không có template",
            TaskStatus = StaffTaskStatus.PENDING,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(2)
        });

        result.Success.Should().BeTrue();
        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(t => t.TaskId == 7104);
        saved.TaskTemplateId.Should().Be(1);
        saved.TaskName.Should().Be("Tên lạ không có template");
    }

    //Function 92 - TC6
    [TestMethod]
    public async Task UpdateAsync_WhenTaskStatusNull_ShouldKeepCurrentStatus()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7105,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "T5",
            TaskStatus = StaffTaskStatus.COMPLETED.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7105, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 3,
            TaskName = "T5 Updated",
            TaskStatus = null,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddDays(2)
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.COMPLETED);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(t => t.TaskId == 7105);
        saved.TaskStatus.Should().Be(StaffTaskStatus.COMPLETED.ToString());
    }

    //Function 92 - TC7
    [TestMethod]
    public async Task UpdateAsync_WhenTaskNameWhitespace_ShouldFallbackToExistingTaskName()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7106,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "Giữ tên cũ",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7106, new OrderDetailStaffTaskUpdateRequest
        {
            OrderDetailId = 9001,
            StaffId = 3,
            TaskName = "   ",
            TaskStatus = StaffTaskStatus.IN_PROGRESS,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddDays(3),
            Note = "  ghi chú mới  "
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskName.Should().Be("Giữ tên cũ");
        result.Data.Note.Should().Be("ghi chú mới");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(t => t.TaskId == 7106);
        saved.TaskName.Should().Be("Giữ tên cũ");
        saved.Note.Should().Be("ghi chú mới");
    }
    #endregion

    #region Function 96 - Staff View Assigned Task
    //Function 96 - TC1
    [TestMethod]
    public async Task GetMyTasksAsync_WhenNoTaskAssigned_ShouldReturnEmpty()
    {
        var result = await _sut.GetMyTasksAsync(staffId: 3, page: 1, pageSize: 10);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 96 - TC2
    [TestMethod]
    public async Task GetMyTasksAsync_WhenTasksExist_ShouldReturnOnlyCurrentStaffTasksWithPaging()
    {
        await SeedTaskAsync(7601, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(3), "A Task");
        await SeedTaskAsync(7602, 3, StaffTaskStatus.IN_PROGRESS, DateTime.UtcNow.AddHours(4), "B Task");
        await SeedTaskAsync(7603, 4, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(5), "Outsider Task");

        var result = await _sut.GetMyTasksAsync(staffId: 3, page: 1, pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().TaskName.Should().Match(x => x == "A Task" || x == "B Task");
        result.Items.First().OrderDetail.OrderDetailId.Should().Be(9001);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
    }

    //Function 96 - TC3
    [TestMethod]
    public async Task GetMyTasksAsync_WhenHasOverdueTask_ShouldAutoMarkOverdueAndNotifyLeader()
    {
        await SeedTaskAsync(7604, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(-2), "Overdue task");

        var result = await _sut.GetMyTasksAsync(staffId: 3, page: 1, pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().TaskStatus.Should().Be((int)StaffTaskStatus.OVERDUE);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7604);
        saved.TaskStatus.Should().Be(StaffTaskStatus.OVERDUE.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                2,
                It.Is<string>(title => title.Contains("trễ deadline")),
                It.Is<string>(body => body.Contains("Overdue task")),
                NotificationType.Task,
                It.Is<Dictionary<string, string>>(d =>
                    d["taskId"] == "7604" &&
                    d["orderDetailId"] == "9001" &&
                    d["taskStatus"] == StaffTaskStatus.OVERDUE.ToString())),
            Times.Once());
    }
    #endregion

    #region Function 97 - Staff Update Task Status
    //Function 97 - TC1
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenTaskNotFound_ShouldFail()
    {
        var result = await _sut.UpdateMyTaskStatusAsync(99999, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.COMPLETED
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy công việc.");
        result.Data.Should().BeNull();
    }

    //Function 97 - TC2
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenTaskBelongsToAnotherStaff_ShouldFail()
    {
        await SeedTaskAsync(7701, 4, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(3), "Other staff task");

        var result = await _sut.UpdateMyTaskStatusAsync(7701, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.IN_PROGRESS
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bạn không có quyền cập nhật công việc này.");
        result.Data.Should().BeNull();
    }

    //Function 97 - TC3
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenMarkCompleted_ShouldUpdateAndNotifyLeader()
    {
        await SeedTaskAsync(7702, 3, StaffTaskStatus.IN_PROGRESS, DateTime.UtcNow.AddHours(2), "Setup hall");

        var result = await _sut.UpdateMyTaskStatusAsync(7702, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.COMPLETED
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Cập nhật trạng thái công việc thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.COMPLETED);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7702);
        saved.TaskStatus.Should().Be(StaffTaskStatus.COMPLETED.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                2,
                It.Is<string>(title => title.Contains("đã hoàn thành công việc")),
                It.Is<string>(body => body.Contains("Setup hall")),
                NotificationType.Task,
                It.Is<Dictionary<string, string>>(d =>
                    d["taskId"] == "7702" &&
                    d["orderDetailId"] == "9001" &&
                    d["staffId"] == "3" &&
                    d["taskStatus"] == StaffTaskStatus.COMPLETED.ToString())),
            Times.Once());
    }

    //Function 97 - TC4
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenRequestInProgressAndStatusUnchanged_ShouldStillNotifyLeader()
    {
        await SeedTaskAsync(7703, 3, StaffTaskStatus.IN_PROGRESS, DateTime.UtcNow.AddHours(2), "In-progress task");

        var result = await _sut.UpdateMyTaskStatusAsync(7703, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.IN_PROGRESS
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.IN_PROGRESS);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7703);
        saved.TaskStatus.Should().Be(StaffTaskStatus.IN_PROGRESS.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                2,
                It.Is<string>(title => title.Contains("đang làm việc")),
                It.Is<string>(body => body.Contains("In-progress task")),
                NotificationType.Task,
                It.Is<Dictionary<string, string>>(d =>
                    d["taskId"] == "7703" &&
                    d["taskStatus"] == StaffTaskStatus.IN_PROGRESS.ToString())),
            Times.Once());
    }

    //Function 97 - TC5
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenTaskOverdueAndRequestNotCompleted_ShouldSetOverdue()
    {
        await SeedTaskAsync(7704, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(-3), "Late task");

        var result = await _sut.UpdateMyTaskStatusAsync(7704, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.IN_PROGRESS
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.OVERDUE);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7704);
        saved.TaskStatus.Should().Be(StaffTaskStatus.OVERDUE.ToString());
    }

    //Function 97 - TC6
    [TestMethod]
    public async Task UpdateMyTaskStatusAsync_WhenTaskOverdueButRequestCompleted_ShouldAllowCompleted()
    {
        await SeedTaskAsync(7705, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(-2), "Finish late task");

        var result = await _sut.UpdateMyTaskStatusAsync(7705, 3, new StaffUpdateTaskStatusRequest
        {
            TaskStatus = StaffTaskStatus.COMPLETED
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.COMPLETED);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7705);
        saved.TaskStatus.Should().Be(StaffTaskStatus.COMPLETED.ToString());
    }
    #endregion

    #region Function 98 - Staff Accept/Complete Task
    [TestMethod]
    public async Task AcceptMyTaskAsync_WhenTaskBelongsToStaff_ShouldMoveToInProgress()
    {
        await SeedTaskAsync(7801, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(2), "Accept me");

        var result = await _sut.AcceptMyTaskAsync(7801, 3);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.IN_PROGRESS);

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7801);
        saved.TaskStatus.Should().Be(StaffTaskStatus.IN_PROGRESS.ToString());
    }

    [TestMethod]
    public async Task CompleteMyTaskAsync_WhenValid_ShouldUploadEvidenceAndMarkCompleted()
    {
        await SeedTaskAsync(7802, 3, StaffTaskStatus.IN_PROGRESS, DateTime.UtcNow.AddHours(2), "Complete me");

        var formFileMock = new Mock<IFormFile>();
        var request = new StaffCompleteTaskRequest
        {
            CompletionImage = formFileMock.Object,
            Note = "Đã hoàn tất"
        };

        var result = await _sut.CompleteMyTaskAsync(7802, 3, request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TaskStatus.Should().Be((int)StaffTaskStatus.COMPLETED);
        result.Data.Img.Should().Be("https://res.cloudinary.com/demo/image/upload/task/evidence.jpg");

        var saved = await _dbContext.OrderDetailStaffTasks.AsNoTracking().FirstAsync(x => x.TaskId == 7802);
        saved.TaskStatus.Should().Be(StaffTaskStatus.COMPLETED.ToString());
        saved.Img.Should().Be(JsonSerializer.Serialize("https://res.cloudinary.com/demo/image/upload/task/evidence.jpg"));

        _imageStorageServiceMock.Verify(x => x.UploadImageAsync(
                formFileMock.Object,
                CloudinaryFolder.Task,
                7802,
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task CompleteMyTaskAsync_WhenTaskNotInProgressOrOverdue_ShouldFail()
    {
        await SeedTaskAsync(7803, 3, StaffTaskStatus.PENDING, DateTime.UtcNow.AddHours(2), "Not started");

        var request = new StaffCompleteTaskRequest
        {
            CompletionImage = new Mock<IFormFile>().Object
        };

        var result = await _sut.CompleteMyTaskAsync(7803, 3, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Chỉ có thể hoàn thành công việc khi trạng thái là IN_PROGRESS hoặc OVERDUE.");
    }
    #endregion

    #region Function 93 - Delete Task
    //Function 93 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenTaskNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99_999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy công việc.");
        result.Data.Should().BeFalse();
    }

    //Function 93 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldRemoveTask()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7201,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "Xóa",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteAsync(7201);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Xóa công việc thành công.");
        result.Data.Should().BeTrue();

        (await _dbContext.OrderDetailStaffTasks.AnyAsync(t => t.TaskId == 7201)).Should().BeFalse();
    }

    //Function 93 - TC3
    [TestMethod]
    public async Task DeleteAsync_WhenDeleteTwice_ShouldFailOnSecondDelete()
    {
        _dbContext.OrderDetailStaffTasks.Add(new OrderDetailStaffTask
        {
            TaskId = 7202,
            OrderDetailId = 9001,
            TaskTemplateId = 1,
            StaffId = 3,
            TaskName = "Xóa hai lần",
            TaskStatus = StaffTaskStatus.PENDING.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(1)
        });
        await _dbContext.SaveChangesAsync();

        var first = await _sut.DeleteAsync(7202);
        var second = await _sut.DeleteAsync(7202);

        first.Success.Should().BeTrue();
        second.Success.Should().BeFalse();
        second.Message.Should().Be("Không tìm thấy công việc.");
        second.Data.Should().BeFalse();
    }
    #endregion
}
