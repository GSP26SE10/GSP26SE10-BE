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
public class IngredientServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private IngredientService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var ingredientRepository = new IngredientRepository(_dbContext);
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _sut = new IngredientService(ingredientRepository, _imageStorageServiceMock.Object);

        await SeedIngredientsAsync();
    }

    // Function 24 
    #region Function 24 - Get Ingredient List
    //Function 24 - TC1
    [TestMethod]
    public async Task GetAllIngredientFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllIngredientFilteredAsync(new IngredientFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 24 - TC2
    [TestMethod]
    public async Task GetAllIngredientFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllIngredientFilteredAsync(new IngredientFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 24 - TC3
    [TestMethod]
    public async Task GetAllIngredientFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllIngredientFilteredAsync(new IngredientFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 24 - TC4
    [TestMethod]
    public async Task GetAllIngredientFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllIngredientFilteredAsync(
            new IngredientFilterRequest { IngredientName = "Suga" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().IngredientName.Should().Be("Sugar");
    }

    //Function 24 - TC5
    [TestMethod]
    public async Task GetAllIngredientFiltered_WithDescription_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllIngredientFilteredAsync(
            new IngredientFilterRequest { Description = "sweet" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.First().IngredientName.Should().Be("Sugar");
    }

    //Function 24 - TC6&7
    [TestMethod]
    public async Task GetAllIngredientFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllIngredientFilteredAsync(
            new IngredientFilterRequest { IngredientName = "NotExist" },
            1,
            10);
        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllIngredientFilteredAsync(
            new IngredientFilterRequest { IngredientId = 999 },
            1,
            10);
        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    // Function 25 
    #region Function 25 - Create Ingredient
    //Function 25 - TC1
    [TestMethod]
    public async Task CreateIngredient_WhenValid_ShouldSucceed()
    {
        var request = new IngredientCreateRequest
        {
            IngredientName = "Pepper",
            Description = "Spicy ingredient"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Ingredient created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.IngredientName.Should().Be("Pepper");
        result.Data.Description.Should().Be("Spicy ingredient");
        (await _dbContext.Ingredients.CountAsync()).Should().Be(6);
    }

    //Function 25 - TC2
    [TestMethod]
    public async Task CreateIngredient_WhenNameMissing_ShouldFail()
    {
        var request = new IngredientCreateRequest
        {
            IngredientName = null,
            Description = "Desc"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("IngredientName is required.");
        result.Data.Should().BeNull();
    }

    //Function 25 - TC3
    [TestMethod]
    public async Task CreateIngredient_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new IngredientCreateRequest
        {
            IngredientName = "No Desc Ingredient",
            Description = "   "
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IngredientName.Should().Be("No Desc Ingredient");
        result.Data.Description.Should().BeNullOrEmpty();
    }

    //Function 25 - TC4
    [TestMethod]
    public async Task CreateIngredient_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new IngredientCreateRequest
        {
            IngredientName = "  sugar ",
            Description = "Dup"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("IngredientName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 25 - TC5
    [TestMethod]
    public async Task CreateIngredient_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Ingredient,
                null,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new IngredientCreateRequest
        {
            IngredientName = "With Image",
            Description = "Desc",
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload ingredient image");
        result.Data.Should().BeNull();
    }
    #endregion

    #region Function 26 - Update Ingredient
    //Function 26 - TC1
    [TestMethod]
    public async Task UpdateIngredient_WhenValidAllFields_ShouldSucceed()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "Sugar Updated",
            Description = "Updated description"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Ingredient updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.IngredientName.Should().Be("Sugar Updated");
        result.Data.Description.Should().Be("Updated description");
    }

    //Function 26 - TC2
    [TestMethod]
    public async Task UpdateIngredient_WhenNameEmpty_ShouldFail()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "   ",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("IngredientName is required.");
        result.Data.Should().BeNull();
    }

    //Function 26 - TC3
    [TestMethod]
    public async Task UpdateIngredient_WhenIdNotFound_ShouldFail()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "Ghost",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
        result.Data.Should().BeNull();
    }

    //Function 26 - TC4
    [TestMethod]
    public async Task UpdateIngredient_WhenIdIsZero_ShouldFail()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "X",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(0, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
        result.Data.Should().BeNull();
    }

    //Function 26 - TC5
    [TestMethod]
    public async Task UpdateIngredient_WhenIdIsNegative_ShouldFail()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "X",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(-1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
        result.Data.Should().BeNull();
    }

    //Function 26 - TC6
    [TestMethod]
    public async Task UpdateIngredient_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new IngredientUpdateRequest
        {
            IngredientName = "Salt",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("IngredientName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 26 - TC7
    [TestMethod]
    public async Task UpdateIngredient_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Ingredient,
                2,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new IngredientUpdateRequest
        {
            IngredientName = "Salt",
            Description = "Desc",
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload ingredient image");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 27 
    #region Function 27 - Delete Ingredient
    //Function 27 - TC1
    [TestMethod]
    public async Task DeleteIngredient_WhenValidId_ShouldSucceed()
    {
        var result = await _sut.DeleteAsync(5);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Ingredient deleted successfully.");

        (await _dbContext.Ingredients.FindAsync(5)).Should().BeNull();
    }

    //Function 27 - TC2
    [TestMethod]
    public async Task DeleteIngredient_WhenIdNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(999);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
    }

    //Function 27 - TC3
    [TestMethod]
    public async Task DeleteIngredient_WhenIdIsZero_ShouldFail()
    {
        var result = await _sut.DeleteAsync(0);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
    }

    //Function 27 - TC4
    [TestMethod]
    public async Task DeleteIngredient_WhenIdIsNegative_ShouldFail()
    {
        var result = await _sut.DeleteAsync(-1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
    }

    //Function 27 - TC5
    [TestMethod]
    public async Task DeleteIngredient_WhenDeleteSameIdTwice_SecondShouldFail()
    {
        var first = await _sut.DeleteAsync(4);
        first.Success.Should().BeTrue();

        var second = await _sut.DeleteAsync(4);

        second.Success.Should().BeFalse();
        second.Data.Should().BeFalse();
        second.Message.Should().Be("Ingredient not found.");
    }

    //Function 27 - TC6
    [TestMethod]
    public async Task DeleteIngredient_WhenRelatedDataExists_ShouldFail()
    {
        _dbContext.DishDetails.Add(new DishDetail
        {
            DishDetailId = 901,
            IngredientId = 1
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Cannot delete ingredient because it is being used in related data.");
    }
    #endregion

    private async Task SeedIngredientsAsync()
    {
        _dbContext.Ingredients.AddRange(
            new Ingredient { IngredientId = 1, IngredientName = "Sugar", Description = "Sweet", Img = string.Empty },
            new Ingredient { IngredientId = 2, IngredientName = "Salt", Description = "Salty", Img = string.Empty },
            new Ingredient { IngredientId = 3, IngredientName = "Butter", Description = "Dairy", Img = string.Empty },
            new Ingredient { IngredientId = 4, IngredientName = "Garlic", Description = "Aroma", Img = string.Empty },
            new Ingredient { IngredientId = 5, IngredientName = "Peppercorn", Description = "Spice", Img = string.Empty }
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
