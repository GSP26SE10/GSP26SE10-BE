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
            _dbContext,
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
            },
            new OrderDetail
            {
                OrderDetailId = 9004,
                OrderId = 900,
                Address = "HN",
                NumberOfGuests = 40,
                Status = OrderDetailStatus.IN_PROGRESS.ToString(),
                StaffGroupId = 10,
                StartTime = start,
                EndTime = start.AddHours(3)
            });

        _dbContext.Services.Add(new Service
        {
            ServiceId = 500,
            ServiceName = "MC Service",
            Description = "MC for event",
            BasePrice = 300000,
            Status = "AVAILABLE",
            Img = "https://example.com/service.jpg",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Services.Add(new Service
        {
            ServiceId = 501,
            ServiceName = "Sound Service",
            Description = "Sound for event",
            BasePrice = 500000,
            Status = "AVAILABLE",
            Img = "https://example.com/service2.jpg",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.OrderServices.Add(new OrderService
        {
            OrderServiceId = 7001,
            OrderDetailId = 9001,
            ServiceId = 500,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.OrderServices.Add(new OrderService
        {
            OrderServiceId = 7002,
            OrderDetailId = 9001,
            ServiceId = 501,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
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

        _dbContext.ExtraChargeCatalogs.Add(new ExtraChargeCatalog
        {
            ExtraChargeCatalogId = 101,
            ChargeType = "OVERTIME",
            Title = "Overtime fee",
            Description = "Overtime service fee",
            Unit = "hour",
            UnitPrice = 150_000,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        _dbContext.ExtraChargeCatalogs.Add(new ExtraChargeCatalog
        {
            ExtraChargeCatalogId = 102,
            ChargeType = "MANUAL_EXTRA",
            Title = "Manual extra fee",
            Description = "Extra fee entered manually by leader",
            Unit = "case",
            UnitPrice = 200_000,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        _dbContext.ExtraChargeCatalogs.Add(new ExtraChargeCatalog
        {
            ExtraChargeCatalogId = 103,
            ChargeType = "INACTIVE_EXTRA",
            Title = "Inactive extra fee",
            Description = "Inactive catalog for testing",
            Unit = "case",
            UnitPrice = 50_000,
            Status = "INACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        _dbContext.ServiceExtraChargeCatalogs.Add(new ServiceExtraChargeCatalog
        {
            ServiceExtraChargeCatalogId = 8001,
            ServiceId = 500,
            ExtraChargeCatalogId = 100,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.ServiceExtraChargeCatalogs.Add(new ServiceExtraChargeCatalog
        {
            ServiceExtraChargeCatalogId = 8002,
            ServiceId = 501,
            ExtraChargeCatalogId = 100,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.ServiceExtraChargeCatalogs.Add(new ServiceExtraChargeCatalog
        {
            ServiceExtraChargeCatalogId = 8003,
            ServiceId = 501,
            ExtraChargeCatalogId = 101,
            CreatedAt = DateTime.UtcNow
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

    [TestMethod]
    public async Task CreateAsync_WhenCatalogIsInactive_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 103,
            Quantity = 1
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog is not active.");
        result.Data.Should().BeNull();
    }

    [TestMethod]
    public async Task CreateAsync_WhenCatalogIsNotMappedToUsedServices_ShouldStillCreateSuccessfully()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 102,
            Quantity = 1
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail extra charge created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.ExtraChargeCatalogId.Should().Be(102);
        result.Data.UnitPrice.Should().Be(200_000);
        result.Data.TotalAmount.Should().Be(200_000);
    }

    [TestMethod]
    public async Task CreateAsync_WhenOrderDetailHasNoService_ShouldStillCreateSuccessfully()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9004,
            ExtraChargeCatalogId = 100,
            Quantity = 2
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail extra charge created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.OrderDetailId.Should().Be(9004);
        result.Data.ExtraChargeCatalogId.Should().Be(100);
        result.Data.TotalAmount.Should().Be(50_000);
    }

    [TestMethod]
    public async Task CreateAsync_WhenOvertimeCatalogAndHasOvertime_ShouldAutoCalculateByOvertimeMinutes()
    {
        var detail = await _dbContext.OrderDetails.FirstAsync(x => x.OrderDetailId == 9001);
        detail.EndTime = DateTime.UtcNow.AddMinutes(-120);
        detail.ActualEndTime = detail.EndTime.Value.AddMinutes(90);
        await _dbContext.SaveChangesAsync();

        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 101, // OVERTIME, unitPrice = 150,000 / hour
            Quantity = 1 // ignored for overtime
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Quantity.Should().Be(90);
        result.Data.TotalAmount.Should().Be(13_500_000); // 150,000 * 90 (billing by minute)
    }

    [TestMethod]
    public async Task CreateAsync_WhenOvertimeCatalogButNoOvertime_ShouldFail()
    {
        var request = new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9004, // no ActualEndTime in seed
            ExtraChargeCatalogId = 101,
            Quantity = 1
        };

        var result = await _sut.CreateAsync(request, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot apply overtime extra charge because order detail has no overtime.");
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

    //Function 94 - TC10
    [TestMethod]
    public async Task GetActiveCatalogByOrderDetailAsync_WhenHasMultipleServices_ShouldReturnDistinctUnionCatalogs()
    {
        var result = await _sut.GetActiveCatalogByOrderDetailAsync(9001);

        result.Should().NotBeNull();
        result.Select(x => x.ExtraChargeCatalogId).Should().BeEquivalentTo(new[] { 100, 101 });
        result.Should().HaveCount(2);
    }

    //Function 94 - TC11
    [TestMethod]
    public async Task GetActiveCatalogByOrderDetailAsync_WhenOrderDetailHasNoService_ShouldReturnEmpty()
    {
        var result = await _sut.GetActiveCatalogByOrderDetailAsync(9002);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    //Function 94 - TC12
    [TestMethod]
    public async Task DeleteAsync_WhenNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail extra charge not found.");
        result.Data.Should().BeFalse();
    }

    //Function 94 - TC13
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var created = await _sut.CreateAsync(new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 1
        }, leaderId: 2);

        created.Success.Should().BeTrue();
        created.Data.Should().NotBeNull();

        var result = await _sut.DeleteAsync(created.Data!.OrderDetailExtraChargeId, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail extra charge deleted successfully.");
        result.Data.Should().BeTrue();

        var stillExists = await _dbContext.OrderDetailExtraCharges
            .AnyAsync(x => x.OrderDetailExtraChargeId == created.Data.OrderDetailExtraChargeId);
        stillExists.Should().BeFalse();
    }

    //Function 94 - TC14
    [TestMethod]
    public async Task UpdateAsync_WhenNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new OrderDetailExtraChargeUpdateRequest
        {
            Quantity = 2
        }, leaderId: 2);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail extra charge not found.");
        result.Data.Should().BeNull();
    }

    //Function 94 - TC15
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var created = await _sut.CreateAsync(new OrderDetailExtraChargeCreateRequest
        {
            OrderDetailId = 9001,
            ExtraChargeCatalogId = 100,
            Quantity = 1,
            Note = "old note"
        }, leaderId: 2);

        created.Success.Should().BeTrue();
        created.Data.Should().NotBeNull();

        var incurredAt = DateTime.UtcNow.AddMinutes(-10);
        var result = await _sut.UpdateAsync(created.Data!.OrderDetailExtraChargeId, new OrderDetailExtraChargeUpdateRequest
        {
            Quantity = 4,
            IncurredAt = incurredAt,
            Note = "  updated note  "
        }, leaderId: 2);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Order detail extra charge updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Quantity.Should().Be(4);
        result.Data.TotalAmount.Should().Be(100_000);
        result.Data.Note.Should().Be("updated note");

        var saved = await _dbContext.OrderDetailExtraCharges.AsNoTracking()
            .FirstAsync(x => x.OrderDetailExtraChargeId == created.Data.OrderDetailExtraChargeId);
        saved.Quantity.Should().Be(4);
        saved.TotalAmount.Should().Be(100_000);
        saved.Note.Should().Be("updated note");
        saved.IncurredAt.Should().BeCloseTo(incurredAt, TimeSpan.FromSeconds(1));
    }
    #endregion
}

