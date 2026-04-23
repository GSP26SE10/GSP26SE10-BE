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
public class MenuDishServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private MenuDishService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new MenuDishService(
            new MenuDishRepository(_dbContext),
            new MenuRepository(_dbContext),
            new DishRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.MenuCategories.Add(new MenuCategory
        {
            MenuCategoryId = 1,
            MenuCategoryName = "Set",
            Description = "desc",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Menus.AddRange(
            new Menu
            {
                MenuId = 1,
                MenuName = "Standard Menu",
                BasePrice = 200_000,
                Status = "AVAILABLE",
                ImgUrl = "[]",
                MenuCategoryId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Menu
            {
                MenuId = 2,
                MenuName = "Premium Menu",
                BasePrice = 350_000,
                Status = "AVAILABLE",
                ImgUrl = "[]",
                MenuCategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });

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
            },
            new Dish
            {
                DishId = 3,
                DishName = "Dish 3",
                Description = "desc",
                Note = string.Empty,
                Img = string.Empty,
                Status = "AVAILABLE",
                Price = 90_000
            });

        _dbContext.MenuDishes.AddRange(
            new MenuDish { MenuDishId = 1001, MenuId = 1, DishId = 1 },
            new MenuDish { MenuDishId = 1002, MenuId = 1, DishId = 2 },
            new MenuDish { MenuDishId = 1003, MenuId = 2, DishId = 3 });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 32 - Get All MenuDish Filtered
    //Function 32 - TC1
    [TestMethod]
    public async Task GetAllMenuDishFilteredAsync_WhenFilterByMenuId_ShouldReturnMatchedRecords()
    {
        var result = await _sut.GetAllMenuDishFilteredAsync(
            new MenuDishFilterRequest { MenuId = 1 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.MenuId == 1);
        result.Items.Select(x => x.DishName).Should().Contain(new[] { "Dish 1", "Dish 2" });
    }

    //Function 32 - TC2
    [TestMethod]
    public async Task GetAllMenuDishFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllMenuDishFilteredAsync(
            new MenuDishFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().MenuDishId.Should().Be(1002);
    }
    #endregion

    #region Function 33 - Create MenuDish
    //Function 33 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenMenuNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MenuDishCreateRequest { MenuId = 999, DishId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
        result.Data.Should().BeNull();
    }

    //Function 33 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenDishNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MenuDishCreateRequest { MenuId = 1, DishId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 33 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenDuplicateMenuDish_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MenuDishCreateRequest { MenuId = 1, DishId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This dish already exists in the selected menu.");
        result.Data.Should().BeNull();
    }

    //Function 33 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new MenuDishCreateRequest { MenuId = 2, DishId = 1 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu dish created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.MenuId.Should().Be(2);
        result.Data.DishId.Should().Be(1);
        result.Data.MenuName.Should().Be("Premium Menu");
        result.Data.DishName.Should().Be("Dish 1");
    }
    #endregion

    #region Function 34 - Update MenuDish
    //Function 34 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenMenuDishNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new MenuDishUpdateRequest { MenuId = 1, DishId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 34 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenMenuNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(1001, new MenuDishUpdateRequest { MenuId = 999, DishId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
        result.Data.Should().BeNull();
    }

    //Function 34 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenDishNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(1001, new MenuDishUpdateRequest { MenuId = 1, DishId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish not found.");
        result.Data.Should().BeNull();
    }

    //Function 34 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenDuplicateAfterUpdate_ShouldFail()
    {
        var result = await _sut.UpdateAsync(1002, new MenuDishUpdateRequest { MenuId = 1, DishId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This dish already exists in the selected menu.");
        result.Data.Should().BeNull();
    }

    //Function 34 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(1003, new MenuDishUpdateRequest { MenuId = 2, DishId = 3 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu dish updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.MenuId.Should().Be(2);
        result.Data.MenuName.Should().Be("Premium Menu");

        var saved = await _dbContext.MenuDishes.AsNoTracking().FirstAsync(x => x.MenuDishId == 1003);
        saved.MenuId.Should().Be(2);
        saved.DishId.Should().Be(3);
    }
    #endregion

    #region Function 35 - Delete MenuDish
    //Function 35 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenMenuDishNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu dish not found.");
        result.Data.Should().BeFalse();
    }

    //Function 35 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(1002);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu dish deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.MenuDishes.AnyAsync(x => x.MenuDishId == 1002)).Should().BeFalse();
    }
    #endregion
}

