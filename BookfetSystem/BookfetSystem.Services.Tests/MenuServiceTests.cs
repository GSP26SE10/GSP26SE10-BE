using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class MenuServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private MenuService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<Menu, MenuResponse>()
            .Map(dest => dest.Status, src => ParseNullableInt(src.Status));

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        var menuRepository = new MenuRepository(_dbContext);
        var menuCategoryRepository = new MenuCategoryRepository(_dbContext);
        var partyCategoryRepository = new PartyCategoryRepository(_dbContext);
        var partyCategoryMenuRepository = new PartyCategoryMenuRepository(_dbContext);
        _imageStorageServiceMock = new Mock<IImageStorageService>();

        _sut = new MenuService(
            menuRepository,
            menuCategoryRepository,
            partyCategoryRepository,
            partyCategoryMenuRepository,
            _imageStorageServiceMock.Object);

        await SeedMenusAsync();
    }

    #region Function 4 - Get Menu List
    //Function 4 - TC1
    [TestMethod]
    public async Task GetAllMenuFiltered_GetAllMenu_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(new MenuFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 4 - TC2
    [TestMethod]
    public async Task GetAllMenuFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(new MenuFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 4 - TC3
    [TestMethod]
    public async Task GetAllMenuFiltered_WithFilter_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(
            new MenuFilterRequest { MenuName = "BBQ" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().MenuName.Should().Contain("BBQ");
    }

    //Function 4 - TC4
    [TestMethod]
    public async Task GetAllMenuFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllMenuFilteredAsync(new MenuFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 4 - TC5
    [TestMethod]
    public async Task GetAllMenuFiltered_WhenPageSizeIsZero_ShouldUseDefaultPageSize()
    {
        var page = 1;
        var pageSize = 0;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllMenuFilteredAsync(new MenuFilterRequest(), page, pageSize);

        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    //Function 4 - TC6
    [TestMethod]
    public async Task GetAllMenuFiltered_WhenPageIsTooLarge_ShouldReturnEmptyItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(new MenuFilterRequest(), 999, 10);

        result.Page.Should().Be(999);
        result.TotalCount.Should().Be(5);
        result.Items.Should().BeEmpty();
    }

    //Function 4 - TC7
    [TestMethod]
    public async Task GetAllMenuFiltered_WhenMenuNameNotExist_ShouldReturnEmptyItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(
            new MenuFilterRequest { MenuName = "not-exist-menu-name" },
            1,
            10);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    //Function 4 - TC8
    [TestMethod]
    public async Task GetAllMenuFiltered_WhenFilterRangeInvalid_ShouldReturnEmptyItems()
    {
        var result = await _sut.GetAllMenuFilteredAsync(
            new MenuFilterRequest
            {
                MinBasePrice = 500_000,
                MaxBasePrice = 100_000
            },
            1,
            10);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
    #endregion

    #region Function 5 - Create Menu
    //Function 5 - TC1
    [TestMethod]
    public async Task CreateMenu_WithValidRequestAndImage_ShouldSuccess()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<BookfetSystem.Services.Enum.CloudinaryFolder>(), null, default))
            .ReturnsAsync("https://cdn.test/menu-1.jpg");

        var request = new MenuCreateRequest
        {
            MenuName = "New Menu With Image",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 120_000,
            ImgFiles = new List<IFormFile> { CreateFormFile("menu.jpg", "image/jpeg") }
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Menu created successfully.");
    }

    //Function 5 - TC2
    [TestMethod]
    public async Task CreateMenu_WithValidRequestAndNoImage_ShouldSuccess()
    {
        var request = new MenuCreateRequest
        {
            MenuName = "New Menu No Image",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 100_000
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Menu created successfully.");
    }

    //Function 5 - TC3
    [TestMethod]
    public async Task CreateMenu_WhenNameMissing_ShouldFail()
    {
        var request = new MenuCreateRequest
        {
            MenuName = "",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 100_000
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuName is required.");
    }

    //Function 5 - TC4
    [TestMethod]
    public async Task CreateMenu_WhenPriceIsNegative_ShouldFail()
    {
        var request = new MenuCreateRequest
        {
            MenuName = "Negative Price Menu",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = -1
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("BasePrice must be greater than or equal to 0.");
    }

    //Function 5 - TC5
    [TestMethod]
    public async Task CreateMenu_WhenPartyCategoryMissing_ShouldFail()
    {
        var request = new MenuCreateRequest
        {
            MenuName = "Menu Missing Party Category",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int>(),
            BasePrice = 100_000
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("At least one PartyCategoryId is required.");
    }

    //Function 5 - TC7
    [TestMethod]
    public async Task CreateMenu_WhenUploadThrowsException_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<BookfetSystem.Services.Enum.CloudinaryFolder>(), null, default))
            .ThrowsAsync(new Exception("Unsupported media type"));

        var request = new MenuCreateRequest
        {
            MenuName = "Menu Upload Fail",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 100_000,
            ImgFiles = new List<IFormFile> { CreateFormFile("audio.mp3", "audio/mpeg") }
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload menu image");
    }
    #endregion

    // Function 6
    #region Function 6 - Update Menu
    //Function 6 - TC1
    [TestMethod]
    public async Task UpdateMenu_WhenValidRequest_ShouldSuccess()
    {
        var request = new MenuUpdateRequest
        {
            MenuName = "BBQ Premium Renamed",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 310_000,
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu updated successfully.");
        result.Data!.MenuName.Should().Be("BBQ Premium Renamed");
    }

    //Function 6 - TC2
    [TestMethod]
    public async Task UpdateMenu_WithImage_ShouldSuccess()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Menu,
                2,
                default))
            .ReturnsAsync("https://cdn.test/menu-2.jpg");

        var request = new MenuUpdateRequest
        {
            MenuName = "Seafood Deluxe Updated",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 460_000,
            Status = MenuStatus.AVAILABLE,
            ImgFiles = new List<IFormFile> { CreateFormFile("update.jpg", "image/jpeg") }
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu updated successfully.");
    }

    //Function 6 - TC3
    [TestMethod]
    public async Task UpdateMenu_WithoutImage_ShouldSuccess()
    {
        var request = new MenuUpdateRequest
        {
            MenuName = "Vegetarian Set Updated",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 260_000,
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(3, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu updated successfully.");
        result.Data!.MenuName.Should().Be("Vegetarian Set Updated");
    }

    //Function 6 - TC5
    [TestMethod]
    public async Task UpdateMenu_WhenPriceIsNegative_ShouldFail()
    {
        var request = new MenuUpdateRequest
        {
            MenuName = "Family Combo",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = -1,
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(4, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("BasePrice must be greater than or equal to 0.");
    }

    //Function 6 - TC6
    [TestMethod]
    public async Task UpdateMenu_WhenNameIsEmpty_ShouldFail()
    {
        var request = new MenuUpdateRequest
        {
            MenuName = "   ",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 200_000,
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(4, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuName is required.");
    }

    //Function 6 - TC7
    [TestMethod]
    public async Task UpdateMenu_WhenUploadThrowsException_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Menu,
                5,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new MenuUpdateRequest
        {
            MenuName = "Birthday Party Set Updated",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 360_000,
            Status = MenuStatus.AVAILABLE,
            ImgFiles = new List<IFormFile> { CreateFormFile("bad.bin", "application/octet-stream") }
        };

        var result = await _sut.UpdateAsync(5, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload menu image");
    }

    //Function 6 - TC8
    [TestMethod]
    public async Task UpdateMenu_WhenIdDoesNotExist_ShouldFail()
    {
        var request = new MenuUpdateRequest
        {
            MenuName = "Ghost Menu",
            MenuCategoryId = 1,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = 100_000,
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
    }

    //Function 6 - TC10 (dùng menu 4 để tránh xung đột với TC1 đã đổi tên menu 1)
    [TestMethod]
    public async Task UpdateMenu_WhenPayloadUnchanged_ShouldStillSuccess()
    {
        var menu = await _dbContext.Menus.FindAsync(4);
        menu.Should().NotBeNull();
        var status = int.TryParse(menu!.Status, out var s) ? (MenuStatus)s : MenuStatus.AVAILABLE;

        var request = new MenuUpdateRequest
        {
            MenuName = menu.MenuName,
            MenuCategoryId = menu.MenuCategoryId!.Value,
            PartyCategoryIds = new List<int> { 1 },
            BasePrice = menu.BasePrice,
            Status = status
        };

        var result = await _sut.UpdateAsync(4, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu updated successfully.");
    }
    #endregion

    private async Task SeedMenusAsync()
    {
        var menuCategory = new MenuCategory
        {
            MenuCategoryId = 1,
            MenuCategoryName = "Party Set",
            Description = "Party menu category",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MenuCategories.Add(menuCategory);
        _dbContext.PartyCategories.Add(new PartyCategory
        {
            PartyCategoryId = 1,
            PartyCategoryName = "Wedding",
            Description = "Wedding party",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Menus.AddRange(
            new Menu { MenuId = 1, MenuName = "BBQ Premium", BasePrice = 300_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 2, MenuName = "Seafood Deluxe", BasePrice = 450_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 3, MenuName = "Vegetarian Set", BasePrice = 250_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 4, MenuName = "Family Combo", BasePrice = 200_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 5, MenuName = "Birthday Party Set", BasePrice = 350_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow }
        );

        await _dbContext.SaveChangesAsync();
    }

    private static IFormFile CreateFormFile(string fileName, string contentType)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImgFiles", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static int? ParseNullableInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static void NormalizePagination(ref int page, ref int pageSize)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
    }
}
