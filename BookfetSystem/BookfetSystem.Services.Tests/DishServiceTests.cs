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
public class DishServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private DishService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var dishRepository = new DishRepository(_dbContext);
        var dishCategoryRepository = new DishCategoryRepository(_dbContext);
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _sut = new DishService(dishRepository, dishCategoryRepository, _imageStorageServiceMock.Object);

        await SeedDishesAsync();
    }

    // Function 12 
    #region Function 12 - Get Dish List
    //Function 12 - TC1
    [TestMethod]
    public async Task GetAllDishFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllDishFilteredAsync(new DishFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 12 - TC2
    [TestMethod]
    public async Task GetAllDishFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllDishFilteredAsync(new DishFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 12 - TC3
    [TestMethod]
    public async Task GetAllDishFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllDishFilteredAsync(new DishFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 12 - TC4
    [TestMethod]
    public async Task GetAllDishFiltered_WithStatusAvailable_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllDishFilteredAsync(
            new DishFilterRequest { Status = DishStatus.AVAILABLE },
            1,
            10);

        result.TotalCount.Should().Be(4);
        result.Items.Should().OnlyContain(x => x.Status == 1);
    }

    //Function 12 - TC5
    [TestMethod]
    public async Task GetAllDishFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllDishFilteredAsync(
            new DishFilterRequest { DishName = "Caes" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().DishName.Should().Be("Caesar Salad");
    }

    //Function 12 - TC6&7
    [TestMethod]
    public async Task GetAllDishFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllDishFilteredAsync(
            new DishFilterRequest { DishName = "NotExist" },
            1,
            10);
        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllDishFilteredAsync(
            new DishFilterRequest { DishId = 999 },
            1,
            10);
        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    // Function 13 
    #region Function 13 - Create Dish
    //Function 13 - TC1
    [TestMethod]
    public async Task CreateDish_WhenValid_ShouldSucceed()
    {
        var request = new DishCreateRequest
        {
            DishName = "New Dish",
            Price = 120_000,
            Description = "Desc",
            Note = "Note",
            DishCategoryId = 1
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishName.Should().Be("New Dish");
        result.Data.Price.Should().Be(120_000);
        result.Data.Status.Should().Be(1);
        result.Data.DishCategoryId.Should().Be(1);
        result.Data.DishCategoryName.Should().Be("Main");

        (await _dbContext.Dishes.CountAsync()).Should().Be(6);
    }

    //Function 13 - TC2
    [TestMethod]
    public async Task CreateDish_WhenNameMissing_ShouldFail()
    {
        var request = new DishCreateRequest
        {
            DishName = null,
            Price = 50_000,
            DishCategoryId = 1
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishName is required.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC3
    [TestMethod]
    public async Task CreateDish_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new DishCreateRequest
        {
            DishName = "No Desc Dish",
            Price = 80_000,
            Description = "   ",
            DishCategoryId = 1
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DishName.Should().Be("No Desc Dish");
        result.Data.Description.Should().BeNullOrEmpty();
    }

    //Function 13 - TC4
    [TestMethod]
    public async Task CreateDish_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new DishCreateRequest
        {
            DishName = "  Grilled Salmon  ",
            Price = 10_000,
            DishCategoryId = 1
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC5
    [TestMethod]
    public async Task CreateDish_WhenDishCategoryNotFound_ShouldFail()
    {
        var request = new DishCreateRequest
        {
            DishName = "Orphan Dish",
            Price = 10_000,
            DishCategoryId = 999
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategory not found.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC6
    [TestMethod]
    public async Task CreateDish_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Dish,
                null,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new DishCreateRequest
        {
            DishName = "With Image",
            Price = 10_000,
            DishCategoryId = 1,
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload dish image");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 14 
    #region Function 14 - Update Dish
    //Function 14 - TC1
    [TestMethod]
    public async Task UpdateDish_WhenValidAllFields_ShouldSucceed()
    {
        var request = new DishUpdateRequest
        {
            DishName = "Spring Rolls Updated",
            Price = 55_000,
            Description = "Updated desc",
            Note = "Updated note",
            DishCategoryId = 1,
            Status = DishStatus.UNAVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishName.Should().Be("Spring Rolls Updated");
        result.Data.Price.Should().Be(55_000);
        result.Data.Status.Should().Be(0);
    }

    //Function 14 - TC2
    [TestMethod]
    public async Task UpdateDish_WhenNameEmpty_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "   ",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishName is required.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC3
    [TestMethod]
    public async Task UpdateDish_WhenIdNotFound_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "Ghost",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC4
    [TestMethod]
    public async Task UpdateDish_WhenIdIsZero_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "X",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(0, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC5
    [TestMethod]
    public async Task UpdateDish_WhenIdIsNegative_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "X",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(-1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC6
    [TestMethod]
    public async Task UpdateDish_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "Tomato Soup",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC7
    [TestMethod]
    public async Task UpdateDish_WhenDishCategoryNotFound_ShouldFail()
    {
        var request = new DishUpdateRequest
        {
            DishName = "Grilled Salmon",
            Price = 10_000,
            DishCategoryId = 999,
            Status = DishStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategory not found.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC8
    [TestMethod]
    public async Task UpdateDish_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Dish,
                3,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new DishUpdateRequest
        {
            DishName = "Caesar Salad",
            Price = 10_000,
            DishCategoryId = 1,
            Status = DishStatus.AVAILABLE,
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.UpdateAsync(3, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload dish image");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 15 
    #region Function 15 - Delete Dish
    //Function 15 - TC1
    [TestMethod]
    public async Task DeleteDish_WhenValidId_ShouldSucceed()
    {
        var result = await _sut.DeleteAsync(5);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Dish deleted successfully.");

        (await _dbContext.Dishes.FindAsync(5)).Should().BeNull();
    }

    //Function 15 - TC2
    [TestMethod]
    public async Task DeleteDish_WhenIdNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(999);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
    }

    //Function 15 - TC3
    [TestMethod]
    public async Task DeleteDish_WhenIdIsZero_ShouldFail()
    {
        var result = await _sut.DeleteAsync(0);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
    }

    //Function 15 - TC4
    [TestMethod]
    public async Task DeleteDish_WhenIdIsNegative_ShouldFail()
    {
        var result = await _sut.DeleteAsync(-1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
    }

    //Function 15 - TC5
    [TestMethod]
    public async Task DeleteDish_WhenDeleteSameIdTwice_SecondShouldFail()
    {
        var first = await _sut.DeleteAsync(4);
        first.Success.Should().BeTrue();

        var second = await _sut.DeleteAsync(4);

        second.Success.Should().BeFalse();
        second.Data.Should().BeFalse();
        second.Message.Should().Be("Dish not found.");
    }

    //Function 15 - TC6
    [TestMethod]
    public async Task DeleteDish_WhenRelatedDataExists_ShouldFail()
    {
        _dbContext.DishDetails.Add(new DishDetail
        {
            DishDetailId = 900,
            DishId = 1,
            IngredientId = null
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Cannot delete dish because it is being used in related data.");
    }
    #endregion

    private async Task SeedDishesAsync()
    {
        _dbContext.DishCategories.Add(new DishCategory
        {
            DishCategoryId = 1,
            DishCategoryName = "Main",
            Description = "Main dishes"
        });

        _dbContext.Dishes.AddRange(
            new Dish { DishId = 1, DishName = "Spring Rolls", Price = 50_000, Description = "Appetizer", Note = "", Img = "", Status = "AVAILABLE", DishCategoryId = 1 },
            new Dish { DishId = 2, DishName = "Grilled Salmon", Price = 150_000, Description = "Fish", Note = "", Img = "", Status = "AVAILABLE", DishCategoryId = 1 },
            new Dish { DishId = 3, DishName = "Caesar Salad", Price = 60_000, Description = "Salad", Note = "", Img = "", Status = "AVAILABLE", DishCategoryId = 1 },
            new Dish { DishId = 4, DishName = "Tomato Soup", Price = 40_000, Description = "Soup", Note = "", Img = "", Status = "AVAILABLE", DishCategoryId = 1 },
            new Dish { DishId = 5, DishName = "Chocolate Cake", Price = 70_000, Description = "Dessert", Note = "", Img = "", Status = "UNAVAILABLE", DishCategoryId = 1 }
        );

        await _dbContext.SaveChangesAsync();
    }

    private static IFormFile CreateFormFile(string fileName, string contentType)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImgFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
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
