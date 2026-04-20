using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class OrderDetailServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private OrderDetailService _sut = null!;
    private Mock<IOrderStatusSchedulerService> _schedulerMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _schedulerMock = new Mock<IOrderStatusSchedulerService>();
        _schedulerMock.Setup(x =>
                x.ScheduleOrderDetailStatusTransitionsAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);
        _schedulerMock.Setup(x =>
                x.ScheduleOrderDepositTimeoutAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);
        _schedulerMock.Setup(x =>
                x.SchedulePendingApprovalAutoCancelAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        _sut = new OrderDetailService(
            new OrderDetailRepository(_dbContext),
            new OrderRepository(_dbContext),
            _schedulerMock.Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Users.Add(new User
        {
            UserId = 1,
            FullName = "Customer One",
            Email = "customer@test.com",
            Phone = "0900000000",
            Avatar = string.Empty,
            Status = "ACTIVE",
            PasswordHash = "hash",
            UserName = "customer1",
            Address = "HN",
            RoleId = 4,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Orders.AddRange(
            new Order
            {
                OrderId = 950,
                CustomerId = 1,
                Status = OrderStatus.IN_PROGRESS.ToString(),
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 3_000_000
            },
            new Order
            {
                OrderId = 951,
                CustomerId = 1,
                Status = OrderStatus.IN_PROGRESS.ToString(),
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 4_000_000
            });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 95 - Leader End Party Early
    //Function 95 - TC1
    [TestMethod]
    public async Task EndEarlyByLeaderAsync_WhenOrderDetailNotFound_ShouldFail()
    {
        var result = await _sut.EndEarlyByLeaderAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail not found.");
        result.Data.Should().BeNull();
    }

    //Function 95 - TC2
    [TestMethod]
    public async Task EndEarlyByLeaderAsync_WhenDetailStatusIsNotInProgress_ShouldFail()
    {
        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 9502,
            OrderId = 950,
            Status = OrderDetailStatus.PREPARING.ToString(),
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddHours(3)
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.EndEarlyByLeaderAsync(9502);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Only IN_PROGRESS order detail can be ended early.");
        result.Data.Should().BeNull();

        var saved = await _dbContext.OrderDetails.AsNoTracking().FirstAsync(x => x.OrderDetailId == 9502);
        saved.Status.Should().Be(OrderDetailStatus.PREPARING.ToString());
    }

    //Function 95 - TC3
    [TestMethod]
    public async Task EndEarlyByLeaderAsync_WhenStillHasOtherIncompleteDetails_ShouldKeepOrderInProgress()
    {
        var oldEnd = DateTime.UtcNow.AddHours(4);
        _dbContext.OrderDetails.AddRange(
            new OrderDetail
            {
                OrderDetailId = 9503,
                OrderId = 950,
                Status = OrderDetailStatus.IN_PROGRESS.ToString(),
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = oldEnd
            },
            new OrderDetail
            {
                OrderDetailId = 9504,
                OrderId = 950,
                Status = OrderDetailStatus.PREPARING.ToString(),
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(6)
            });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.EndEarlyByLeaderAsync(9503);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail ended early successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be((int)OrderDetailStatus.COMPLETED);

        var detail = await _dbContext.OrderDetails.AsNoTracking().FirstAsync(x => x.OrderDetailId == 9503);
        detail.Status.Should().Be(OrderDetailStatus.COMPLETED.ToString());
        detail.EndTime.Should().NotBeNull();
        detail.EndTime!.Value.Should().BeBefore(oldEnd);

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 950);
        order.Status.Should().Be(OrderStatus.IN_PROGRESS.ToString());
    }

    //Function 95 - TC4
    [TestMethod]
    public async Task EndEarlyByLeaderAsync_WhenAllDetailsCompleted_ShouldMoveOrderToBilling()
    {
        var futureEnd = DateTime.UtcNow.AddHours(5);
        _dbContext.OrderDetails.AddRange(
            new OrderDetail
            {
                OrderDetailId = 9511,
                OrderId = 951,
                Status = OrderDetailStatus.IN_PROGRESS.ToString(),
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = futureEnd
            },
            new OrderDetail
            {
                OrderDetailId = 9512,
                OrderId = 951,
                Status = OrderDetailStatus.COMPLETED.ToString(),
                StartTime = DateTime.UtcNow.AddHours(-4),
                EndTime = DateTime.UtcNow.AddHours(-1)
            });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.EndEarlyByLeaderAsync(9511);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail ended early successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be((int)OrderDetailStatus.COMPLETED);

        var detail = await _dbContext.OrderDetails.AsNoTracking().FirstAsync(x => x.OrderDetailId == 9511);
        detail.Status.Should().Be(OrderDetailStatus.COMPLETED.ToString());
        detail.EndTime.Should().NotBeNull();
        detail.EndTime!.Value.Should().BeBefore(futureEnd);

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 951);
        order.Status.Should().Be(OrderStatus.BILLING.ToString());
    }
    #endregion
}

