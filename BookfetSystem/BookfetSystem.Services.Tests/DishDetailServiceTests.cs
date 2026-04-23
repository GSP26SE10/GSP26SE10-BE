using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class DishDetailServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private DishDetailService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new DishDetailService(
            new DishDetailRepository(_dbContext),
            new DishRepository(_dbContext),
            new IngredientRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Dishes.AddRange(
            new Dish
            {
                DishId = 1,
                DishName = "Dish 1",
                Description = "desc",
                Note = string.Empty,
                Img = string.Empty,
                Status = "AVAILABLE",
                Price = 50_000
            },
            new Dish
            {
                DishId = 2,
                DishName = "Dish 2",
                Description = "desc",
                Note = string.Empty,
                Img = string.Empty,
                Status = "AVAILABLE",
                Price = 70_000
            });

        _dbContext.Ingredients.AddRange(
            new Ingredient
            {
                IngredientId = 1,
                IngredientName = "Ingredient 1",
                Description = "desc",
                Img = string.Empty
            },
            new Ingredient
            {
                IngredientId = 2,
                IngredientName = "Ingredient 2",
                Description = "desc",
                Img = string.Empty
            },
            new Ingredient
            {
                IngredientId = 3,
                IngredientName = "Ingredient 3",
                Description = "desc",
                Img = string.Empty
            });

        _dbContext.DishDetails.AddRange(
            new DishDetail { DishDetailId = 2001, DishId = 1, IngredientId = 1 },
            new DishDetail { DishDetailId = 2002, DishId = 1, IngredientId = 2 },
            new DishDetail { DishDetailId = 2003, DishId = 2, IngredientId = 3 });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 36 - Get All DishDetail Filtered
    //Function 36 - TC1
    [TestMethod]
    public async Task GetAllDishDetailFilteredAsync_WhenFilterByDishId_ShouldReturnMatchedRecords()
    {
        var result = await _sut.GetAllDishDetailFilteredAsync(
            new DishDetailFilterRequest { DishId = 1 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.DishId == 1);
        result.Items.Select(x => x.IngredientName).Should().Contain(new[] { "Ingredient 1", "Ingredient 2" });
    }

    //Function 36 - TC2
    [TestMethod]
    public async Task GetAllDishDetailFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllDishDetailFilteredAsync(
            new DishDetailFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().DishDetailId.Should().Be(2002);
    }
    #endregion

    #region Function 37 - Create DishDetail
    //Function 37 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenDishNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new DishDetailCreateRequest { DishId = 999, IngredientId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 37 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenIngredientNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new DishDetailCreateRequest { DishId = 1, IngredientId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
        result.Data.Should().BeNull();
    }

    //Function 37 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenDuplicateDishDetail_ShouldFail()
    {
        var result = await _sut.CreateAsync(new DishDetailCreateRequest { DishId = 1, IngredientId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This ingredient already exists in the selected dish.");
        result.Data.Should().BeNull();
    }

    //Function 37 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new DishDetailCreateRequest { DishId = 2, IngredientId = 2 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish detail created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishId.Should().Be(2);
        result.Data.IngredientId.Should().Be(2);
        result.Data.DishName.Should().Be("Dish 2");
        result.Data.IngredientName.Should().Be("Ingredient 2");
    }
    #endregion

    #region Function 38 - Update DishDetail
    //Function 38 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenDishDetailNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new DishDetailUpdateRequest { DishId = 1, IngredientId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish detail not found.");
        result.Data.Should().BeNull();
    }

    //Function 38 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenDishNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(2001, new DishDetailUpdateRequest { DishId = 999, IngredientId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 38 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenIngredientNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(2001, new DishDetailUpdateRequest { DishId = 1, IngredientId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Ingredient not found.");
        result.Data.Should().BeNull();
    }

    //Function 38 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenDuplicateAfterUpdate_ShouldFail()
    {
        var result = await _sut.UpdateAsync(2002, new DishDetailUpdateRequest { DishId = 1, IngredientId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This ingredient already exists in the selected dish.");
        result.Data.Should().BeNull();
    }

    //Function 38 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(2003, new DishDetailUpdateRequest { DishId = 2, IngredientId = 2 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish detail updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishId.Should().Be(2);
        result.Data.IngredientId.Should().Be(2);
        result.Data.DishName.Should().Be("Dish 2");
        result.Data.IngredientName.Should().Be("Ingredient 2");
    }
    #endregion

    #region Function 39 - Delete DishDetail
    //Function 39 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenDishDetailNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish detail not found.");
        result.Data.Should().BeFalse();
    }

    //Function 39 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(2002);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish detail deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.DishDetails.AnyAsync(x => x.DishDetailId == 2002)).Should().BeFalse();
    }
    #endregion
}

