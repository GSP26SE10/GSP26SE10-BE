using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class OrderDetailExtraChargeServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private OrderDetailExtraChargeService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn.test/extra/default.jpg");

        _sut = new OrderDetailExtraChargeService(
            new OrderDetailExtraChargeRepository(_dbContext),
            new OrderDetailRepository(_dbContext),
            new ExtraChargeCatalogRepository(_dbContext),
            new StaffGroupRepository(_dbContext),
            _imageStorageServiceMock.Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
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
                UserId = 4,
                FullName = "Other Leader",
                Email = "otherleader@test.com",
                Phone = "0900000004",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "leader4",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.StaffGroups.AddRange(
            new StaffGroup
            {
                StaffGroupId = 10,
                StaffGroupName = "Service Team",
                Status = StaffGroupStatus.ACTIVE.ToString(),
                LeaderId = 2
            },
            new StaffGroup
            {
                StaffGroupId = 11,
                StaffGroupName = "Inactive Team",
                Status = StaffGroupStatus.INACTIVE.ToString(),
                LeaderId = 4
            });

        _dbContext.Orders.Add(new Order
        {
            OrderId = 900,
            CustomerId = 1,
            Status = OrderStatus.IN_PROGRESS.ToString(),
            TotalPrice = 5_000_000,
            DepositAmount = 1_000_000,
            RemainingAmount = 0,
            CreatedAt = DateTime.UtcNow
        });

        var start = DateTime.UtcNow.AddDays(-1);
        _dbContext.OrderDetails.AddRange(
            new OrderDetail
            {
                OrderDetailId = 9001,
                OrderId = 900,
                Address = "HN",
                NumberOfGuests = 50,
                Status = OrderDetailStatus.IN_PROGRESS.ToString(),
                StaffGroupId = 10,
                StartTime = start,
                EndTime = start.AddHours(4)
            },
            new OrderDetail
            {
                OrderDetailId = 9002,
                OrderId = 900,
                Address = "HN",
                NumberOfGuests = 50,
                Status = OrderDetailStatus.PENDING.ToString(),
                StaffGroupId = 10,
                StartTime = start,
                EndTime = start.AddHours(4)
            },
            new OrderDetail
            {
                OrderDetailId = 9003,
                OrderId = 900,
                Address = "HN",
                NumberOfGuests = 50,
                Status = OrderDetailStatus.IN_PROGRESS.ToString(),
                StaffGroupId = 99,
                StartTime = start,
                EndTime = start.AddHours(4)
            });

        _dbContext.ExtraChargeCatalogs.Add(new ExtraChargeCatalog
        {
            ExtraChargeCatalogId = 100,
            ChargeType = "BROKEN_ITEM",
            Title = "Broken plate",
            Description = "Broken plate fee",
            Unit = "item",
            UnitPrice = 25_000,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        return new FormFile(stream, 0, stream.Length, "ImageFiles", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    #region Function 94 - Group Leader Add Extra Charge
    //Function 94 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenLeaderHasNoActiveGroup_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Leader does not have an active staff group.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailNotFound_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 99999,
            ExtraChargeCatalogId = 100,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail not found.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailNotInLeaderGroup_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9003,
            ExtraChargeCatalogId = 100,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail does not belong to your staff group.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailStatusNotAllowed_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9002,
            ExtraChargeCatalogId = 100,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge can only be created when order detail is IN_PROGRESS or COMPLETED.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenCatalogNotFound_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 99999,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog not found.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC6
    [TestMethod]
    public async Task CreateAsync_WhenImageCountExceedsLimit_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 2,
            ImageFiles = new List<IFormFile>
            {
                CreateFormFile("1.jpg"),
                CreateFormFile("2.jpg"),
                CreateFormFile("3.jpg"),
                CreateFormFile("4.jpg"),
                CreateFormFile("5.jpg"),
                CreateFormFile("6.jpg")
            }
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("You can upload up to 5 images at once.");
        result.Data.Should().BeNull();

        _imageStorageServiceMock.Verify(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()),
            Times.Never());
    }

    //Function 94 - TC7
    [TestMethod]
    public async Task CreateAsync_WhenImageUploadFails_ShouldFail()
    {
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("upload error"));

        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 2,
            ImageFiles = new List<IFormFile> { CreateFormFile("1.jpg") }
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload extra charge images: upload error");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC8
    [TestMethod]
    public async Task CreateAsync_WhenValidWithoutImages_ShouldCreateSuccessfully()
    {
        var incurredAt = DateTime.UtcNow.AddHours(-1);
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 3,
            IncurredAt = incurredAt,
            Note = "  Broken at setup  "
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail extra charge created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.OrderDetailId.Should().Be(9001);
        result.Data.ExtraChargeCatalogId.Should().Be(100);
        result.Data.UnitPrice.Should().Be(25_000);
        result.Data.Quantity.Should().Be(3);
        result.Data.TotalAmount.Should().Be(75_000);
        result.Data.CreateBy.Should().Be(2);
        result.Data.Note.Should().Be("Broken at setup");

        var saved = await _dbContext.OrderDetailExtraCharges.AsNoTracking()
            .FirstAsync(x => x.OrderDetailId == 9001 && x.ExtraChargeCatalogId == 100);
        saved.TotalAmount.Should().Be(75_000);
        saved.Note.Should().Be("Broken at setup");
        saved.Image.Should().BeNull();
    }

    //Function 94 - TC9
    [TestMethod]
    public async Task CreateAsync_WhenValidWithImages_ShouldUploadAndStoreImageJson()
    {
        _imageStorageServiceMock.SetupSequence(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.ExtraCharge,
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn.test/extra/1.jpg")
            .ReturnsAsync("https://cdn.test/extra/2.jpg");

        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 1,
            ImageFiles = new List<IFormFile>
            {
                CreateFormFile("1.jpg"),
                CreateFormFile("2.jpg")
            }
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var saved = await _dbContext.OrderDetailExtraCharges.AsNoTracking()
            .OrderByDescending(x => x.OrderDetailExtraChargeId)
            .FirstAsync();
        saved.Image.Should().Contain("https://cdn.test/extra/1.jpg");
        saved.Image.Should().Contain("https://cdn.test/extra/2.jpg");

        _imageStorageServiceMock.Verify(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.ExtraCharge,
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    #endregion
}

