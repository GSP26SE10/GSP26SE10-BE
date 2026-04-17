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
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class FeedbackMenuServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private FeedbackMenuService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _imageStorageServiceMock = new Mock<IImageStorageService>();

        var apiKeyProviderMock = new Mock<IApiKeyProvider>();
        apiKeyProviderMock.Setup(x => x.GetRandomKey()).Returns("fake-key");
        var geminiService = new GeminiService(new HttpClient(), apiKeyProviderMock.Object);

        _sut = new FeedbackMenuService(
            new FeedbackMenuRepository(_dbContext),
            new MenuRepository(_dbContext),
            new UserRepository(_dbContext),
            new OrderRepository(_dbContext),
            new OrderDetailRepository(_dbContext),
            _imageStorageServiceMock.Object,
            geminiService);

        await SeedBaseDataAsync();
    }

    #region Function 84 - Feedback Menu
    //Function 84 - TC1
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenOrderNotFound_ShouldFail()
    {
        var request = BuildValidRequest();
        request.OrderId = 999;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found.");
    }

    //Function 84 - TC2
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenMenuNotFound_ShouldFail()
    {
        var request = BuildValidRequest();
        request.MenuId = 999;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
    }

    //Function 84 - TC3
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenOrderDetailNotFound_ShouldFail()
    {
        var request = BuildValidRequest();
        request.OrderDetailId = 999;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail not found.");
    }

    //Function 84 - TC4
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenOrderDetailNotBelongOrder_ShouldFail()
    {
        _dbContext.Orders.Add(new Order
        {
            OrderId = 2,
            CustomerId = 1,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = BuildValidRequest();
        request.OrderId = 2; // detail 1 belongs to order 1

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order detail does not belong to the specified order.");
    }

    //Function 84 - TC5
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenMenuNotBelongOrderDetail_ShouldFail()
    {
        _dbContext.Menus.Add(new Menu
        {
            MenuId = 2,
            MenuName = "Menu 2",
            Status = "AVAILABLE",
            ImgUrl = "[]",
            MenuCategoryId = 1
        });
        await _dbContext.SaveChangesAsync();

        var request = BuildValidRequest();
        request.MenuId = 2; // order detail 1 has menu 1

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu does not belong to the specified order detail.");
    }

    //Function 84 - TC6
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenCustomerNotFound_ShouldFail()
    {
        var request = BuildValidRequest();
        request.CustomerId = 999;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Customer not found.");
    }

    //Function 84 - TC7
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenUploadMoreThanThreeImages_ShouldFail()
    {
        var request = BuildValidRequest();
        request.ImgFiles = new List<IFormFile>
        {
            CreateFormFile("1.jpg"), CreateFormFile("2.jpg"), CreateFormFile("3.jpg"), CreateFormFile("4.jpg")
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("You can upload up to 3 images at once.");
    }

    //Function 84 - TC8
    [TestMethod]
    public async Task CreateFeedbackMenu_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.FeedbackMenu,
                null,
                default))
            .ThrowsAsync(new Exception("invalid file"));

        var request = BuildValidRequest();
        request.ImgFiles = new List<IFormFile> { CreateFormFile("bad.jpg") };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload feedback menu images");
    }
    #endregion

    private FeedbackMenuCreateRequest BuildValidRequest()
    {
        return new FeedbackMenuCreateRequest
        {
            OrderId = 1,
            OrderDetailId = 1,
            MenuId = 1,
            CustomerId = 1,
            Rating = 5,
            Comment = "Great menu"
        };
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
            MenuName = "Menu 1",
            Status = "AVAILABLE",
            ImgUrl = "[]",
            MenuCategoryId = 1
        });

        _dbContext.Orders.Add(new Order
        {
            OrderId = 1,
            CustomerId = 1,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 1,
            OrderId = 1,
            MenuId = 1,
            PartyCategoryId = null,
            NumberOfGuests = 20,
            Address = "HN",
            Status = "PENDING",
            StartTime = DateTime.UtcNow.AddDays(4),
            EndTime = DateTime.UtcNow.AddDays(4).AddHours(2)
        });

        await _dbContext.SaveChangesAsync();
    }

    private static IFormFile CreateFormFile(string fileName)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImgFiles", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}

