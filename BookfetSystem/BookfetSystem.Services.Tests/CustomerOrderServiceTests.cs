using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using BookfetSystem.Services.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System.Linq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class CustomerOrderServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private CustomerOrderService _sut = null!;

    private Mock<IOrderStatusSchedulerService> _orderStatusSchedulerMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IPaymentService> _paymentServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _orderStatusSchedulerMock = new Mock<IOrderStatusSchedulerService>();
        _orderStatusSchedulerMock.Setup(x =>
                x.ScheduleOrderDetailStatusTransitionsAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);
        _orderStatusSchedulerMock.Setup(x =>
                x.ScheduleOrderDepositTimeoutAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);
        _orderStatusSchedulerMock.Setup(x =>
                x.SchedulePendingApprovalAutoCancelAsync(It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock.Setup(x => x.SendToUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock = new Mock<IEmailService>();
        _emailServiceMock.Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _paymentServiceMock = new Mock<IPaymentService>();
        _paymentServiceMock.Setup(x => x.RefundRejectedOrderDepositAsync(It.IsAny<int>(), It.IsAny<string?>()))
            .ReturnsAsync(new ApiResponse<object> { Success = true, Data = new { amount = 0m } });
        _paymentServiceMock.Setup(x => x.RefundOrderDepositByAmountAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>()))
            .ReturnsAsync(new ApiResponse<object> { Success = true, Data = new { amount = 0m } });

        _sut = new CustomerOrderService(
            _dbContext,
            new OrderRepository(_dbContext),
            new UserRepository(_dbContext),
            new OrderDetailRepository(_dbContext),
            new OrderServiceRepository(_dbContext),
            new ServiceRepository(_dbContext),
            new OrderDetailCustomRepository(_dbContext),
            new DishRepository(_dbContext),
            new StaffGroupRepository(_dbContext),
            new MenuRepository(_dbContext),
            new MenuDishRepository(_dbContext),
            new PartyCategoryRepository(_dbContext),
            _orderStatusSchedulerMock.Object,
            _notificationServiceMock.Object,
            _emailServiceMock.Object,
            _paymentServiceMock.Object);

        await SeedBaseDataAsync();
    }

    #region Function 79 - Create Order (Customer)
    //Function 79 - TC1
    [TestMethod]
    public async Task CreateOrderAsync_WhenCustomerNotFound_ShouldFail()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 999,
            Items = new List<CreateOrderItemRequest>()
        };

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy khách hàng.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC2
    [TestMethod]
    public async Task CreateOrderAsync_WhenNoItems_ShouldFail()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>()
        };

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cần có ít nhất một mục trong đơn.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC3
    [TestMethod]
    public async Task CreateOrderAsync_WhenPartyDateLessThanThreeDays_ShouldFail()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new()
                {
                    MenuId = 1,
                    PartyCategoryId = 1,
                    NumberOfGuests = 20,
                    Address = "HN",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(3)
                }
            }
        };

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Ngày tiệc đầu tiên phải cách hôm nay ít nhất 3 ngày");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC4
    [TestMethod]
    public async Task CreateOrderAsync_WhenEndTimeLessThanOrEqualStartTime_OnPrecheck_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].EndTime = request.Items[0].StartTime;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Mục 1: Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC5
    [TestMethod]
    public async Task CreateOrderAsync_WhenPartyDatesTooFarFromFirstParty_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items.Add(new CreateOrderItemRequest
        {
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 20,
            Address = "HN",
            StartTime = request.Items[0].StartTime.AddDays(3),
            EndTime = request.Items[0].StartTime.AddDays(3).AddHours(2)
        });

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Tất cả ngày tiệc phải nằm trong vòng 1 ngày tính từ ngày tiệc đầu tiên.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC6
    [TestMethod]
    public async Task CreateOrderAsync_WhenMenuIdInvalid_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].MenuId = 0;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuId phải lớn hơn 0.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC7
    [TestMethod]
    public async Task CreateOrderAsync_WhenNumberOfGuestsInvalid_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].NumberOfGuests = 0;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Số lượng khách phải lớn hơn 0.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC8
    [TestMethod]
    public async Task CreateOrderAsync_WhenMenuNotFound_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].MenuId = 999;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy menu có mã 999.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC9
    [TestMethod]
    public async Task CreateOrderAsync_WhenPartyCategoryIdInvalid_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].PartyCategoryId = 0;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("PartyCategoryId là bắt buộc và phải lớn hơn 0.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC10
    [TestMethod]
    public async Task CreateOrderAsync_WhenPartyCategoryNotFound_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].PartyCategoryId = 999;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy loại tiệc có mã 999.");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC11
    [TestMethod]
    public async Task CreateOrderAsync_WhenGuestsLessThanPartyCategoryRequirement_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].PartyCategoryId = 2;
        request.Items[0].NumberOfGuests = 10;

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Số lượng khách phải lớn hơn hoặc bằng 30");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC12
    [TestMethod]
    public async Task CreateOrderAsync_WhenCustomDishAlreadyInMenu_ShouldFail()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].CustomDishes = new List<CustomDishItemRequest>
        {
            new() { DishId = 1 }
        };

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists in menu");
        result.Data.Should().Be(0);
    }

    //Function 79 - TC13
    [TestMethod]
    public async Task CreateOrderAsync_WhenValidRequest_ShouldSucceed()
    {
        var request = BuildValidCreateOrderRequest();
        request.Items[0].Services = new List<ServiceItemRequest>
        {
            new() { ServiceId = 1, Quantity = 2 }
        };

        var result = await _sut.CreateOrderAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order created successfully.");
        result.Data.Should().BeGreaterThan(0);

        var order = await _dbContext.Orders
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.OrderId == result.Data);
        order.Should().NotBeNull();
        order!.OrderDetails.Should().HaveCount(1);
        order.TotalPrice.Should().BeGreaterThan(0);
    }
    #endregion

    #region Function 86 - Get Order List
    //Function 86 - TC1
    [TestMethod]
    public async Task GetAllFilteredAsync_WhenNoFilter_ShouldReturnAllOrders()
    {
        await SeedOrdersForListAsync();

        var result = await _sut.GetAllFilteredAsync(new OrderFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 86 - TC2
    [TestMethod]
    public async Task GetAllFilteredAsync_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        await SeedOrdersForListAsync();

        var result = await _sut.GetAllFilteredAsync(new OrderFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 86 - TC3
    [TestMethod]
    public async Task GetAllFilteredAsync_WhenPageOrPageSizeInvalid_ShouldNormalizeLikeApi()
    {
        await SeedOrdersForListAsync();

        var page = 0;
        var pageSize = 0;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllFilteredAsync(new OrderFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(3);
    }

    //Function 86 - TC4
    [TestMethod]
    public async Task GetAllFilteredAsync_WithStatusPending_ShouldReturnMatched()
    {
        await SeedOrdersForListAsync();

        var result = await _sut.GetAllFilteredAsync(
            new OrderFilterRequest { Status = OrderStatus.PENDING },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Status.Should().Be((int)OrderStatus.PENDING);
    }

    //Function 86 - TC5
    [TestMethod]
    public async Task GetAllFilteredAsync_WithNotFoundOrderId_ShouldReturnEmpty()
    {
        await SeedOrdersForListAsync();

        var result = await _sut.GetAllFilteredAsync(
            new OrderFilterRequest { OrderId = 9999 },
            1,
            10);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
    #endregion

    #region Function 87 - Accept / Reject Order (Review)
    //Function 87 - TC1
    [TestMethod]
    public async Task ReviewOrderAsync_WhenReviewerInvalid_ShouldFail()
    {
        var result = await _sut.ReviewOrderAsync(1, (int)OrderStatus.APPROVED, reviewerId: 0, noteOrder: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Mã người duyệt không hợp lệ.");
        result.Data.Should().BeNull();
    }

    //Function 87 - TC2
    [TestMethod]
    public async Task ReviewOrderAsync_WhenStatusNotDefined_ShouldFail()
    {
        var result = await _sut.ReviewOrderAsync(1, status: 99, reviewerId: 10, noteOrder: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Giá trị trạng thái không hợp lệ.");
        result.Data.Should().BeNull();
    }

    //Function 87 - TC3
    [TestMethod]
    public async Task ReviewOrderAsync_WhenStatusNotApprovedOrRejected_ShouldFail()
    {
        var result = await _sut.ReviewOrderAsync(1, (int)OrderStatus.PENDING, reviewerId: 10, noteOrder: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Chỉ cho phép trạng thái APPROVED (2) hoặc REJECTED (3).");
        result.Data.Should().BeNull();
    }

    //Function 87 - TC4
    [TestMethod]
    public async Task ReviewOrderAsync_WhenOrderNotFound_ShouldFail()
    {
        var result = await _sut.ReviewOrderAsync(9999, (int)OrderStatus.APPROVED, reviewerId: 10, noteOrder: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy đơn hàng.");
        result.Data.Should().BeNull();
    }

    //Function 87 - TC5
    [TestMethod]
    public async Task ReviewOrderAsync_WhenOrderNotPending_ShouldFail()
    {
        await CreateOrderWithOneDetailAsync(601, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(5));

        var result = await _sut.ReviewOrderAsync(601, (int)OrderStatus.APPROVED, reviewerId: 10, noteOrder: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Chỉ đơn hàng ở trạng thái chờ duyệt mới có thể được duyệt.");
        result.Data.Should().BeNull();
    }

    //Function 87 - TC6
    [TestMethod]
    public async Task ReviewOrderAsync_WhenRejectAndRefundFails_ShouldNotUpdateOrder()
    {
        await CreateOrderWithOneDetailAsync(602, OrderStatus.PENDING.ToString(), DateTime.UtcNow.AddDays(6), depositAmount: 500_000m);

        _paymentServiceMock
            .Setup(x => x.RefundRejectedOrderDepositAsync(602, It.IsAny<string?>()))
            .ReturnsAsync(new ApiResponse<object> { Success = false, Message = "ZaloPay timeout" });

        var result = await _sut.ReviewOrderAsync(602, (int)OrderStatus.REJECTED, reviewerId: 10, noteOrder: " Lý do ");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không thể từ chối đơn hàng vì hoàn tiền chưa hoàn tất: ZaloPay timeout");
        result.Data.Should().BeNull();

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 602);
        order.Status.Should().Be(OrderStatus.PENDING.ToString());
        order.ReviewedBy.Should().BeNull();
        order.NoteOrder.Should().BeNull();

        var detail = await _dbContext.OrderDetails.AsNoTracking().FirstAsync(x => x.OrderId == 602);
        detail.Status.Should().Be(OrderDetailStatus.APPROVED.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Dictionary<string, string>>()),
            Times.Never());
        _emailServiceMock.Verify(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()),
            Times.Never());
    }

    //Function 87 - TC7
    [TestMethod]
    public async Task ReviewOrderAsync_WhenRejectAndRefundSucceeds_ShouldUpdateOrderAndDetails()
    {
        await CreateOrderWithOneDetailAsync(603, OrderStatus.PENDING.ToString(), DateTime.UtcNow.AddDays(7), depositAmount: 500_000m);

        _paymentServiceMock
            .Setup(x => x.RefundRejectedOrderDepositAsync(603, It.IsAny<string?>()))
            .ReturnsAsync(new ApiResponse<object> { Success = true, Data = new { amount = 250_000m } });

        var result = await _sut.ReviewOrderAsync(603, (int)OrderStatus.REJECTED, reviewerId: 11, noteOrder: "Không đủ nhân sự");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Đơn hàng đã bị từ chối thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be((int)OrderStatus.REJECTED);
        result.Data.ReviewedBy.Should().Be(11);
        result.Data.NoteOrder.Should().Be("Không đủ nhân sự");

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 603);
        order.Status.Should().Be(OrderStatus.REJECTED.ToString());

        var details = await _dbContext.OrderDetails.AsNoTracking().Where(x => x.OrderId == 603).ToListAsync();
        details.Should().NotBeEmpty();
        details.Should().OnlyContain(x => x.Status == OrderDetailStatus.REJECTED.ToString());

        _paymentServiceMock.Verify(x => x.RefundRejectedOrderDepositAsync(603, "Không đủ nhân sự"), Times.Once());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                1,
                "Đơn tiệc đã bị từ chối",
                It.IsAny<string>(),
                NotificationType.Order,
                It.Is<Dictionary<string, string>>(d =>
                    d["orderId"] == "603" && d["status"] == OrderStatus.REJECTED.ToString())),
            Times.Once());

        _emailServiceMock.Verify(x => x.SendAsync(
                "customer@test.com",
                "[Bookfet] Đơn hàng đã bị từ chối",
                It.IsAny<string>(),
                It.IsAny<string?>()),
            Times.Once());
    }

    //Function 87 - TC8
    [TestMethod]
    public async Task ReviewOrderAsync_WhenApprove_ShouldUpdateOrderAndNotifyCustomer()
    {
        await CreateOrderWithOneDetailAsync(604, OrderStatus.PENDING.ToString(), DateTime.UtcNow.AddDays(8));

        var result = await _sut.ReviewOrderAsync(604, (int)OrderStatus.APPROVED, reviewerId: 12, noteOrder: "   ");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Đơn hàng đã được duyệt thành công.");
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be((int)OrderStatus.APPROVED);
        result.Data.ReviewedBy.Should().Be(12);
        result.Data.NoteOrder.Should().BeNull();

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 604);
        order.Status.Should().Be(OrderStatus.APPROVED.ToString());

        var detail = await _dbContext.OrderDetails.AsNoTracking().FirstAsync(x => x.OrderId == 604);
        detail.Status.Should().Be(OrderDetailStatus.APPROVED.ToString());

        _paymentServiceMock.Verify(x => x.RefundRejectedOrderDepositAsync(It.IsAny<int>(), It.IsAny<string?>()), Times.Never());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                1,
                "Đơn tiệc đã được duyệt",
                It.IsAny<string>(),
                NotificationType.Order,
                It.Is<Dictionary<string, string>>(d =>
                    d["orderId"] == "604" && d["status"] == OrderStatus.APPROVED.ToString())),
            Times.Once());

        _emailServiceMock.Verify(x => x.SendAsync(
                "customer@test.com",
                "[Bookfet] Đơn hàng đã được duyệt",
                It.IsAny<string>(),
                It.IsAny<string?>()),
            Times.Once());
    }
    #endregion

    #region Function 88 - Assign Order to Staff Group
    //Function 88 - TC1
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenStaffGroupIdInvalid_ShouldFail()
    {
        var result = await _sut.AssignOrderToStaffGroupAsync(1, staffGroupId: 0);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("StaffGroupId must be greater than 0.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC2
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenOrderNotFound_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();

        var result = await _sut.AssignOrderToStaffGroupAsync(999_999, staffGroupId: 50);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC3
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenOrderNotApproved_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(831, OrderStatus.PENDING.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: 100_000);

        var result = await _sut.AssignOrderToStaffGroupAsync(831, staffGroupId: 50);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Only APPROVED orders can be assigned.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC4
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenNotDeposited_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(832, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: null);

        var result = await _sut.AssignOrderToStaffGroupAsync(832, staffGroupId: 50);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order has not been deposited yet.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC5
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenStaffGroupInactive_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(833, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: 100_000);

        var result = await _sut.AssignOrderToStaffGroupAsync(833, staffGroupId: 52);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group not found or inactive.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC6
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenStaffGroupHasNoLeader_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(834, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: 100_000);

        var result = await _sut.AssignOrderToStaffGroupAsync(834, staffGroupId: 51);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group must have a leader before assignment.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC7
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenOrderHasNoDetails_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        _dbContext.Orders.Add(new Order
        {
            OrderId = 835,
            CustomerId = 1,
            Status = OrderStatus.APPROVED.ToString(),
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 1_000_000,
            DepositAmount = 200_000
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.AssignOrderToStaffGroupAsync(835, staffGroupId: 50);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order has no order details to assign.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC8
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenAlreadyAssigned_ShouldFail()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(836, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: 100_000);
        var detail = await _dbContext.OrderDetails.FirstAsync(x => x.OrderId == 836);
        detail.StaffGroupId = 50;
        await _dbContext.SaveChangesAsync();

        var result = await _sut.AssignOrderToStaffGroupAsync(836, staffGroupId: 50);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order is already assigned to a staff group.");
        result.Data.Should().BeNull();
    }

    //Function 88 - TC9
    [TestMethod]
    public async Task AssignOrderToStaffGroupAsync_WhenValid_ShouldAssignDetailsNotifyLeaderAndSetPreparing()
    {
        await SeedStaffGroupForAssignmentTestsAsync();
        await CreateOrderWithOneDetailAsync(830, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(10), depositAmount: 100_000);

        var result = await _sut.AssignOrderToStaffGroupAsync(830, staffGroupId: 50);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order assigned to staff group successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be((int)OrderStatus.PREPARING);

        var order = await _dbContext.Orders.AsNoTracking().FirstAsync(x => x.OrderId == 830);
        order.Status.Should().Be(OrderStatus.PREPARING.ToString());

        var details = await _dbContext.OrderDetails.AsNoTracking().Where(x => x.OrderId == 830).ToListAsync();
        details.Should().OnlyContain(x => x.StaffGroupId == 50);
        details.Should().OnlyContain(x => x.Status == OrderDetailStatus.PREPARING.ToString());

        _notificationServiceMock.Verify(x => x.SendToUserAsync(
                20,
                "Bạn được giao tiệc mới",
                It.Is<string>(body => body.Contains("Wedding") && body.Contains("Standard Menu")),
                NotificationType.Order,
                It.Is<Dictionary<string, string>>(d =>
                    d["orderId"] == "830" && d["staffGroupId"] == "50")),
            Times.Once());
    }
    #endregion

    #region Function 80 - Cancel Customer Order
    //Function 80 - TC1
    [TestMethod]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldFail()
    {
        var result = await _sut.CancelOrderAsync(999, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không tìm thấy đơn hàng.");
        result.Data.Should().BeNull();
    }

    //Function 80 - TC2
    [TestMethod]
    public async Task CancelOrderAsync_WhenNoPermission_ShouldFail()
    {
        var order = new Order
        {
            OrderId = 200,
            CustomerId = 1,
            Status = OrderStatus.PENDING.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CancelOrderAsync(200, actorRoleId: 2, actorUserId: 88, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bạn không có quyền hủy đơn hàng này.");
        result.Data.Should().BeNull();
    }

    //Function 80 - TC3
    [TestMethod]
    public async Task CancelOrderAsync_WhenOrderAlreadyCancelled_ShouldReturnSuccess()
    {
        await CreateOrderWithOneDetailAsync(201, OrderStatus.CANCELLED.ToString(), DateTime.UtcNow.AddDays(8));

        var result = await _sut.CancelOrderAsync(201, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Đơn hàng đã được hủy trước đó.");
        result.Data.Should().NotBeNull();
        result.Data!.OrderId.Should().Be(201);
    }

    //Function 80 - TC4
    [TestMethod]
    public async Task CancelOrderAsync_WhenOrderCompleted_ShouldFail()
    {
        await CreateOrderWithOneDetailAsync(202, OrderStatus.COMPLETED.ToString(), DateTime.UtcNow.AddDays(8));

        var result = await _sut.CancelOrderAsync(202, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không thể hủy đơn hàng đã hoàn thành.");
        result.Data.Should().BeNull();
    }

    //Function 80 - TC5
    [TestMethod]
    public async Task CancelOrderAsync_WhenAnyDetailInProgress_ShouldFail()
    {
        await CreateOrderWithOneDetailAsync(
            203,
            OrderStatus.APPROVED.ToString(),
            DateTime.UtcNow.AddDays(8),
            detailStatus: OrderDetailStatus.IN_PROGRESS.ToString());

        var result = await _sut.CancelOrderAsync(203, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Không thể hủy đơn hàng vì có ít nhất một tiệc đang ở trạng thái đang thực hiện.");
        result.Data.Should().BeNull();
    }

    //Function 80 - TC6
    [TestMethod]
    public async Task CancelOrderAsync_WhenNoValidPartySchedule_ShouldFail()
    {
        var order = new Order
        {
            OrderId = 204,
            CustomerId = 1,
            Status = OrderStatus.APPROVED.ToString(),
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 1_000_000,
            DepositAmount = 200_000
        };
        _dbContext.Orders.Add(order);
        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 2041,
            OrderId = 204,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 20,
            Address = "HN",
            Status = OrderDetailStatus.APPROVED.ToString(),
            StartTime = null,
            EndTime = null
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CancelOrderAsync(204, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Đơn hàng không có lịch tiệc hợp lệ để áp dụng chính sách hủy.");
        result.Data.Should().BeNull();
    }

    //Function 80 - TC7
    [TestMethod]
    public async Task CancelOrderAsync_WhenCustomerCancelsPendingOrder_ShouldRefund100PercentWithoutPaymentCall()
    {
        await CreateOrderWithOneDetailAsync(205, OrderStatus.PENDING.ToString(), DateTime.UtcNow.AddDays(8), depositAmount: 300_000);

        var result = await _sut.CancelOrderAsync(205, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest { Reason = "Changed plan" });

        result.Success.Should().BeTrue();
        result.Message.Should().MatchRegex("Tỷ lệ hoàn cọc: 100\\s*%");

        _paymentServiceMock.Verify(x =>
            x.RefundOrderDepositByAmountAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>()),
            Times.Never());
    }

    //Function 80 - TC8
    [TestMethod]
    public async Task CancelOrderAsync_WhenApprovedAndBetween3To7Days_ShouldRequest50PercentRefund()
    {
        await CreateOrderWithOneDetailAsync(206, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(4), depositAmount: 200_000);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 2061,
            OrderId = 206,
            Amount = 200_000,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
            PaymentStatus = PaymentStatus.PAID.ToString(),
            PaidAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CancelOrderAsync(206, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest { Reason = "Changed plan" });

        result.Success.Should().BeTrue();
        result.Message.Should().MatchRegex("Tỷ lệ hoàn cọc: 50\\s*%");

        _paymentServiceMock.Verify(x =>
            x.RefundOrderDepositByAmountAsync(206, 100_000m, It.IsAny<string?>()),
            Times.Once());
    }

    //Function 80 - TC9
    [TestMethod]
    public async Task CancelOrderAsync_WhenApprovedAndLessThan3Days_ShouldRefundZero()
    {
        await CreateOrderWithOneDetailAsync(207, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(1), depositAmount: 200_000);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 2071,
            OrderId = 207,
            Amount = 200_000,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
            PaymentStatus = PaymentStatus.PAID.ToString(),
            PaidAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CancelOrderAsync(207, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeTrue();
        result.Message.Should().MatchRegex("Tỷ lệ hoàn cọc: 0\\s*%");

        _paymentServiceMock.Verify(x =>
            x.RefundOrderDepositByAmountAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>()),
            Times.Never());
    }

    //Function 80 - TC10
    [TestMethod]
    public async Task CancelOrderAsync_WhenRefundFails_ShouldReturnFailure()
    {
        await CreateOrderWithOneDetailAsync(208, OrderStatus.APPROVED.ToString(), DateTime.UtcNow.AddDays(8), depositAmount: 200_000);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 2081,
            OrderId = 208,
            Amount = 200_000,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
            PaymentStatus = PaymentStatus.PAID.ToString(),
            PaidAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        _paymentServiceMock.Setup(x => x.RefundOrderDepositByAmountAsync(208, It.IsAny<decimal>(), It.IsAny<string?>()))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = false,
                Message = "gateway error",
                Data = null
            });

        var result = await _sut.CancelOrderAsync(208, actorRoleId: 4, actorUserId: 1, new CancelOrderRequest());

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("hoàn tiền chưa hoàn tất");
    }
    #endregion

    #region Function 81 - View Order Detail
    //Function 81 - TC1
    [TestMethod]
    public async Task GetById_WhenOrderNotFound_ShouldReturnNull()
    {
        var result = await _sut.GetById(999);

        result.Should().BeNull();
    }

    //Function 81 - TC2
    [TestMethod]
    public async Task GetById_WhenOrderExists_ShouldReturnBasicInfo()
    {
        var order = new Order
        {
            OrderId = 300,
            CustomerId = 1,
            Status = OrderStatus.PENDING.ToString(),
            TotalPrice = 1_000_000,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetById(300);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(300);
        result.CustomerId.Should().Be(1);
        result.Status.Should().Be((int)OrderStatus.PENDING);
    }
    #endregion

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

        _dbContext.Dishes.Add(new Dish
        {
            DishId = 1,
            DishName = "Dish 1",
            Description = "desc",
            Note = string.Empty,
            Img = string.Empty,
            Status = "AVAILABLE",
            Price = 50_000
        });

        _dbContext.MenuDishes.Add(new MenuDish
        {
            MenuDishId = 1,
            MenuId = 1,
            DishId = 1
        });

        _dbContext.Services.Add(new Service
        {
            ServiceId = 1,
            ServiceName = "MC",
            Description = "Host",
            BasePrice = 100_000,
            Status = "AVAILABLE",
            Img = string.Empty,
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

        _dbContext.PartyCategories.Add(new PartyCategory
        {
            PartyCategoryId = 2,
            PartyCategoryName = "Big Wedding",
            Description = "desc",
            Status = "AVAILABLE",
            NumberOfGuests = 30,
            ImageUrl = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    private static CreateOrderRequest BuildValidCreateOrderRequest()
    {
        var start = DateTime.UtcNow.AddDays(4);
        return new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new()
                {
                    MenuId = 1,
                    PartyCategoryId = 1,
                    NumberOfGuests = 20,
                    Address = "HN",
                    StartTime = start,
                    EndTime = start.AddHours(3)
                }
            }
        };
    }

    private async Task SeedAssignmentLeaderUserAsync()
    {
        if (await _dbContext.Users.AnyAsync(u => u.UserId == 20))
        {
            return;
        }

        _dbContext.Users.Add(new User
        {
            UserId = 20,
            FullName = "Leader Twenty",
            Email = "leader20@test.com",
            Phone = "0900000020",
            Avatar = string.Empty,
            Status = "ACTIVE",
            PasswordHash = "hash",
            UserName = "leader20",
            Address = "HN",
            RoleId = 3,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedStaffGroupForAssignmentTestsAsync()
    {
        await SeedAssignmentLeaderUserAsync();

        if (await _dbContext.StaffGroups.AnyAsync(s => s.StaffGroupId == 50))
        {
            return;
        }

        _dbContext.StaffGroups.AddRange(
            new StaffGroup
            {
                StaffGroupId = 50,
                StaffGroupName = "Team A",
                Status = StaffGroupStatus.ACTIVE.ToString(),
                LeaderId = 20
            },
            new StaffGroup
            {
                StaffGroupId = 51,
                StaffGroupName = "No Leader Team",
                Status = StaffGroupStatus.ACTIVE.ToString(),
                LeaderId = null
            },
            new StaffGroup
            {
                StaffGroupId = 52,
                StaffGroupName = "Inactive Team",
                Status = StaffGroupStatus.INACTIVE.ToString(),
                LeaderId = 20
            });

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedOrdersForListAsync()
    {
        if (await _dbContext.Orders.AnyAsync(x => x.OrderId >= 400 && x.OrderId <= 402))
        {
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.Orders.AddRange(
            new Order
            {
                OrderId = 400,
                CustomerId = 1,
                Status = OrderStatus.PENDING.ToString(),
                TotalPrice = 100_000,
                CreatedAt = now.AddMinutes(-3)
            },
            new Order
            {
                OrderId = 401,
                CustomerId = 1,
                Status = OrderStatus.APPROVED.ToString(),
                TotalPrice = 200_000,
                CreatedAt = now.AddMinutes(-2)
            },
            new Order
            {
                OrderId = 402,
                CustomerId = 1,
                Status = OrderStatus.CANCELLED.ToString(),
                TotalPrice = 300_000,
                CreatedAt = now.AddMinutes(-1)
            });

        await _dbContext.SaveChangesAsync();
    }

    private static void NormalizePagination(ref int page, ref int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
    }

    private async Task CreateOrderWithOneDetailAsync(
        int orderId,
        string orderStatus,
        DateTime? startTime,
        string detailStatus = "APPROVED",
        decimal? depositAmount = null)
    {
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = 1,
            Status = orderStatus,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 1_000_000,
            DepositAmount = depositAmount,
            RemainingAmount = 0
        };
        _dbContext.Orders.Add(order);

        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = orderId * 10 + 1,
            OrderId = orderId,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 20,
            Address = "HN",
            Status = detailStatus,
            StartTime = startTime,
            EndTime = startTime?.AddHours(3)
        });

        await _dbContext.SaveChangesAsync();
    }
}

