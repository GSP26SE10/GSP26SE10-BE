using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class PaymentServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private PaymentService _sut = null!;

    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _emailServiceMock = new Mock<IEmailService>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());

        _sut = new PaymentService(
            _dbContext,
            new PaymentRepository(_dbContext),
            new OrderRepository(_dbContext),
            BuildConfiguration(),
            _emailServiceMock.Object,
            _httpClientFactoryMock.Object);

        await SeedBaseDataAsync();
    }

    #region Function 81 - Pay Deposit (CreateDepositQR)
    //Function 81 - TC1
    [TestMethod]
    public async Task CreateDepositQR_WhenOrderNotFound_ShouldFail()
    {
        var result = await _sut.CreateDepositQR(99999, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found");
    }

    //Function 81 - TC2
    [TestMethod]
    public async Task CreateDepositQR_WhenPaymentMethodCash_ShouldFail()
    {
        await SeedOrderAsync(8201, status: OrderStatus.PENDING.ToString(), totalPrice: 1_000_000m);

        var result = await _sut.CreateDepositQR(8201, PaymentMethod.CASH);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cash payment is not supported for deposit. Please choose BANK_TRANSFER or ZALOPAY.");
    }

    //Function 81 - TC3
    [TestMethod]
    public async Task CreateDepositQR_WhenUnpaidBankTransferAlreadyExists_ShouldReuseExistingQr()
    {
        await SeedOrderAsync(8202, status: OrderStatus.PENDING.ToString(), totalPrice: 1_000_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 82021,
            OrderId = 8202,
            Amount = 500_000m,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var beforeCount = await _dbContext.Payments.CountAsync(x => x.OrderId == 8202);
        var result = await _sut.CreateDepositQR(8202, PaymentMethod.BANK_TRANSFER);
        var afterCount = await _dbContext.Payments.CountAsync(x => x.OrderId == 8202);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("QR already exists for this order. Use existing payment.");
        afterCount.Should().Be(beforeCount);
    }

    //Function 81 - TC4
    [TestMethod]
    public async Task CreateDepositQR_WhenUnpaidZaloPayExistsAndRequestBankTransfer_ShouldCancelOldAndCreateNew()
    {
        await SeedOrderAsync(8203, status: OrderStatus.PENDING.ToString(), totalPrice: 1_000_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 82031,
            OrderId = 8203,
            Amount = 500_000m,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateDepositQR(8203, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("QR created");

        var payments = await _dbContext.Payments.Where(x => x.OrderId == 8203).ToListAsync();
        payments.Should().HaveCount(2);
        payments.Should().Contain(x => x.PaymentMethod == PaymentMethod.ZALOPAY.ToString() && x.PaymentStatus == PaymentStatus.CANCELLED.ToString());
        payments.Should().Contain(x =>
            x.PaymentMethod == PaymentMethod.BANK_TRANSFER.ToString() &&
            x.PaymentType == PaymentType.DEPOSIT.ToString() &&
            x.PaymentStatus == PaymentStatus.UNPAID.ToString() &&
            x.Amount == 500_000m);
    }

    //Function 81 - TC5
    [TestMethod]
    public async Task CreateDepositQR_WhenUnpaidOtherMethodExists_ShouldFail()
    {
        await SeedOrderAsync(8204, status: OrderStatus.PENDING.ToString(), totalPrice: 1_000_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 82041,
            OrderId = 8204,
            Amount = 500_000m,
            PaymentType = PaymentType.DEPOSIT.ToString(),
            PaymentMethod = "UNKNOWN_METHOD",
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateDepositQR(8204, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("An unpaid deposit already exists with method UNKNOWN_METHOD. Please complete or cancel that payment first.");
    }

    //Function 81 - TC6
    [TestMethod]
    public async Task CreateDepositQR_WhenValidBankTransfer_ShouldCreateDepositPaymentAtFiftyPercent()
    {
        await SeedOrderAsync(8205, status: OrderStatus.PENDING.ToString(), totalPrice: 900_000m);

        var result = await _sut.CreateDepositQR(8205, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("QR created");

        var payment = await _dbContext.Payments.SingleAsync(x => x.OrderId == 8205);
        payment.PaymentType.Should().Be(PaymentType.DEPOSIT.ToString());
        payment.PaymentMethod.Should().Be(PaymentMethod.BANK_TRANSFER.ToString());
        payment.PaymentStatus.Should().Be(PaymentStatus.UNPAID.ToString());
        payment.Amount.Should().Be(450_000m);
    }

    //Function 81 - TC7
    [TestMethod]
    public async Task CreateDepositQR_WhenZaloPayWithoutRequiredConfig_ShouldFailEarly()
    {
        var service = new PaymentService(
            _dbContext,
            new PaymentRepository(_dbContext),
            new OrderRepository(_dbContext),
            BuildConfigurationWithoutZalo(),
            _emailServiceMock.Object,
            _httpClientFactoryMock.Object);

        await SeedOrderAsync(8206, status: OrderStatus.PENDING.ToString(), totalPrice: 900_000m);

        var result = await service.CreateDepositQR(8206, PaymentMethod.ZALOPAY);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Missing ZaloPay configuration. Please set ZaloPay:AppId and ZaloPay:Key1.");
    }
    #endregion

    #region Function 82 - Pay Full (CreateFullQR)
    //Function 82 - TC1
    [TestMethod]
    public async Task CreateFullQR_WhenOrderNotFound_ShouldFail()
    {
        var result = await _sut.CreateFullQR(99999, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found");
    }

    //Function 82 - TC2
    [TestMethod]
    public async Task CreateFullQR_WhenOrderNotBilling_ShouldFail()
    {
        await SeedOrderAsync(8301, status: OrderStatus.APPROVED.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m);

        var result = await _sut.CreateFullQR(8301, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order must be BILLING before creating full payment QR.");
    }

    //Function 82 - TC3
    [TestMethod]
    public async Task CreateFullQR_WhenNoRemainingAndNoExtraCharge_ShouldFail()
    {
        await SeedOrderAsync(8302, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 1_000_000m, remainingAmount: 0m);

        var result = await _sut.CreateFullQR(8302, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order has no remaining amount to pay.");
    }

    //Function 82 - TC4
    [TestMethod]
    public async Task CreateFullQR_WhenAlreadyPaidFull_ShouldFail()
    {
        await SeedOrderAsync(8303, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 83031,
            OrderId = 8303,
            Amount = 700_000m,
            PaymentType = PaymentType.FULL.ToString(),
            PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
            PaymentStatus = PaymentStatus.PAID.ToString(),
            PaidAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateFullQR(8303, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order is already fully paid.");
    }

    //Function 82 - TC5
    [TestMethod]
    public async Task CreateFullQR_WhenPaymentMethodCash_ShouldFail()
    {
        await SeedOrderAsync(8304, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);

        var result = await _sut.CreateFullQR(8304, PaymentMethod.CASH);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cash method is not supported in full QR. Use BANK_TRANSFER or ZALOPAY, or call create-full-cash endpoint.");
    }

    //Function 82 - TC6
    [TestMethod]
    public async Task CreateFullQR_WhenValidBankTransfer_ShouldCreateWithRemainingPlusExtraCharge()
    {
        await SeedOrderAsync(8305, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);
        await SeedOrderDetailExtraChargeAsync(orderId: 8305, orderDetailId: 83051, amount: 120_000m);

        var result = await _sut.CreateFullQR(8305, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Full QR created");

        var payment = await _dbContext.Payments.SingleAsync(x => x.OrderId == 8305);
        payment.PaymentType.Should().Be(PaymentType.FULL.ToString());
        payment.PaymentMethod.Should().Be(PaymentMethod.BANK_TRANSFER.ToString());
        payment.PaymentStatus.Should().Be(PaymentStatus.UNPAID.ToString());
        payment.Amount.Should().Be(820_000m);
    }

    //Function 82 - TC7
    [TestMethod]
    public async Task CreateFullQR_WhenUnpaidBankTransferExists_ShouldUpdateAmountAndReuse()
    {
        await SeedOrderAsync(8306, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);
        await SeedOrderDetailExtraChargeAsync(orderId: 8306, orderDetailId: 83061, amount: 50_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 83061,
            OrderId = 8306,
            Amount = 111_000m,
            PaymentType = PaymentType.FULL.ToString(),
            PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateFullQR(8306, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("QR already exists for this order. Use existing full payment.");

        var payments = await _dbContext.Payments.Where(x => x.OrderId == 8306).ToListAsync();
        payments.Should().HaveCount(1);
        payments.Single().Amount.Should().Be(750_000m);
    }

    //Function 82 - TC8
    [TestMethod]
    public async Task CreateFullQR_WhenUnpaidZaloExistsAndRequestBankTransfer_ShouldCancelOldAndCreateNew()
    {
        await SeedOrderAsync(8307, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 83071,
            OrderId = 8307,
            Amount = 700_000m,
            PaymentType = PaymentType.FULL.ToString(),
            PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateFullQR(8307, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeTrue();

        var payments = await _dbContext.Payments.Where(x => x.OrderId == 8307).ToListAsync();
        payments.Should().HaveCount(2);
        payments.Should().Contain(x => x.PaymentMethod == PaymentMethod.ZALOPAY.ToString() && x.PaymentStatus == PaymentStatus.CANCELLED.ToString());
        payments.Should().Contain(x => x.PaymentMethod == PaymentMethod.BANK_TRANSFER.ToString() && x.PaymentStatus == PaymentStatus.UNPAID.ToString());
    }

    //Function 82 - TC9
    [TestMethod]
    public async Task CreateFullQR_WhenUnpaidOtherMethodExists_ShouldFail()
    {
        await SeedOrderAsync(8308, status: OrderStatus.BILLING.ToString(), totalPrice: 1_000_000m, depositAmount: 300_000m, remainingAmount: 700_000m);
        _dbContext.Payments.Add(new Payment
        {
            PaymentId = 83081,
            OrderId = 8308,
            Amount = 700_000m,
            PaymentType = PaymentType.FULL.ToString(),
            PaymentMethod = "UNKNOWN_METHOD",
            PaymentStatus = PaymentStatus.UNPAID.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateFullQR(8308, PaymentMethod.BANK_TRANSFER);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("An unpaid full payment already exists with method UNKNOWN_METHOD. Please complete or cancel that payment first.");
    }
    #endregion

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SePay:QrBaseUrl"] = "https://qr.sepay.vn/img",
                ["SePay:QrAccountNumber"] = "123456789",
                ["SePay:QrBankCode"] = "VCB",
                ["ZaloPay:AppId"] = "2553",
                ["ZaloPay:Key1"] = "test_key_1",
                ["ZaloPay:CreateOrderUrl"] = "https://example.com/create",
                ["ZaloPay:CallbackUrl"] = "https://example.com/callback",
                ["ZaloPay:RedirectUrl"] = "https://example.com/redirect"
            })
            .Build();
    }

    private static IConfiguration BuildConfigurationWithoutZalo()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SePay:QrBaseUrl"] = "https://qr.sepay.vn/img",
                ["SePay:QrAccountNumber"] = "123456789",
                ["SePay:QrBankCode"] = "VCB"
            })
            .Build();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Users.Add(new User
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
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedOrderAsync(
        int orderId,
        string status,
        decimal totalPrice,
        decimal? depositAmount = null,
        decimal? remainingAmount = null)
    {
        _dbContext.Orders.Add(new Order
        {
            OrderId = orderId,
            CustomerId = 1,
            Status = status,
            TotalPrice = totalPrice,
            DepositAmount = depositAmount,
            RemainingAmount = remainingAmount,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedOrderDetailExtraChargeAsync(int orderId, int orderDetailId, decimal amount)
    {
        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = orderDetailId,
            OrderId = orderId,
            MenuId = null,
            PartyCategoryId = null,
            NumberOfGuests = 10,
            Address = "HN",
            Status = OrderDetailStatus.IN_PROGRESS.ToString(),
            Type = OrderDetailType.ORDER.ToString(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2)
        });
        await _dbContext.SaveChangesAsync();

        _dbContext.OrderDetailExtraCharges.Add(new OrderDetailExtraCharge
        {
            OrderDetailExtraChargeId = orderDetailId * 10 + 1,
            OrderDetailId = orderDetailId,
            ChargeType = "MANUAL",
            Title = "Extra",
            Description = "Extra fee",
            Unit = "Lần",
            UnitPrice = amount,
            Quantity = 1,
            TotalAmount = amount,
            Status = "ACTIVE",
            CreateBy = 1,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }
}

