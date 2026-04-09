using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class MenuCategoryServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private MenuCategoryService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var menuCategoryRepository = new MenuCategoryRepository(_dbContext);
        _sut = new MenuCategoryService(menuCategoryRepository);

        await SeedMenuCategoriesAsync();
    }

    // Function 8 
    #region Function 8 - Get Menu Category List
    //Function 8 - TC1
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllMenuCategoryFilteredAsync(new MenuCategoryFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 8 - TC2
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllMenuCategoryFilteredAsync(new MenuCategoryFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 8 - TC3
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllMenuCategoryFilteredAsync(new MenuCategoryFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 8 - TC5
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_WithStatusAvailable_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllMenuCategoryFilteredAsync(
            new MenuCategoryFilterRequest { Status = MenuStatus.AVAILABLE },
            1,
            10);

        result.TotalCount.Should().Be(4);
        result.Items.Should().OnlyContain(x => x.Status == 1);
    }

    //Function 8 - TC6
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllMenuCategoryFilteredAsync(
            new MenuCategoryFilterRequest { MenuCategoryName = "Sea" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().MenuCategoryName.Should().Be("Seafood Set");
    }

    //Function 8 - TC7
    [TestMethod]
    public async Task GetAllMenuCategoryFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllMenuCategoryFilteredAsync(
            new MenuCategoryFilterRequest { MenuCategoryName = "NotExist" },
            1,
            10);

        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllMenuCategoryFilteredAsync(
            new MenuCategoryFilterRequest { MenuCategoryId = 999 },
            1,
            10);

        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    // Function 9 
    #region Function 9 - Create Menu Category
    //Function 9 - TC1
    [TestMethod]
    public async Task CreateMenuCategory_WhenValid_ShouldSucceed()
    {
        var request = new MenuCategoryCreateRequest
        {
            MenuCategoryName = "New Category",
            Description = "New category description"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu category created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.MenuCategoryName.Should().Be("New Category");
        result.Data.Description.Should().Be("New category description");
        result.Data.Status.Should().Be(1);

        (await _dbContext.MenuCategories.CountAsync()).Should().Be(6);
        (await _dbContext.MenuCategories.AnyAsync(x => x.MenuCategoryName == "New Category")).Should().BeTrue();
    }

    //Function 9 - TC2
    [TestMethod]
    public async Task CreateMenuCategory_WhenNameMissing_ShouldFail()
    {
        var request = new MenuCategoryCreateRequest
        {
            MenuCategoryName = null,
            Description = "Desc"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 9 - TC3
    [TestMethod]
    public async Task CreateMenuCategory_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new MenuCategoryCreateRequest
        {
            MenuCategoryName = "No Desc Category",
            Description = "   "
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu category created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.MenuCategoryName.Should().Be("No Desc Category");
        result.Data.Description.Should().BeNullOrEmpty();
        result.Data.Status.Should().Be(1);
    }
    #endregion

    // Function 10 
    #region Function 10 - Update Menu Category
    //Function 10 - TC1
    [TestMethod]
    public async Task UpdateMenuCategory_WhenValidAllFields_ShouldSucceed()
    {
        var request = new MenuCategoryUpdateRequest
        {
            MenuCategoryName = "BBQ Set Updated",
            Description = "Updated description",
            Status = MenuStatus.UNAVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Menu category updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.MenuCategoryName.Should().Be("BBQ Set Updated");
        result.Data.Description.Should().Be("Updated description");
        result.Data.Status.Should().Be(0);
    }

    //Function 10 - TC4
    [TestMethod]
    public async Task UpdateMenuCategory_WhenNameEmpty_ShouldFail()
    {
        var request = new MenuCategoryUpdateRequest
        {
            MenuCategoryName = "   ",
            Description = "Updated description",
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 10 - TC5
    [TestMethod]
    public async Task UpdateMenuCategory_WhenIdNotFound_ShouldFail()
    {
        var request = new MenuCategoryUpdateRequest
        {
            MenuCategoryName = "Ghost category",
            Description = "Updated description",
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu category not found.");
        result.Data.Should().BeNull();
    }

    //Function 10 - TC6
    [TestMethod]
    public async Task UpdateMenuCategory_WhenIdIsZero_ShouldFail()
    {
        var request = new MenuCategoryUpdateRequest
        {
            MenuCategoryName = "Zero id category",
            Description = "Updated description",
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(0, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu category not found.");
        result.Data.Should().BeNull();
    }

    //Function 10 - TC7
    [TestMethod]
    public async Task UpdateMenuCategory_WhenIdIsNegative_ShouldFail()
    {
        var request = new MenuCategoryUpdateRequest
        {
            MenuCategoryName = "Negative id category",
            Description = "Updated description",
            Status = MenuStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(-1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu category not found.");
        result.Data.Should().BeNull();
    }
    #endregion

    private async Task SeedMenuCategoriesAsync()
    {
        _dbContext.MenuCategories.AddRange(
            new MenuCategory { MenuCategoryId = 1, MenuCategoryName = "BBQ Set", Description = "BBQ party set", Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 2, MenuCategoryName = "Seafood Set", Description = "Seafood party set", Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 3, MenuCategoryName = "Vegetarian Set", Description = "Vegetarian party set", Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 4, MenuCategoryName = "Family Set", Description = "Family set", Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 5, MenuCategoryName = "Archived Set", Description = "Old set", Status = "UNAVAILABLE", CreatedAt = DateTime.UtcNow }
        );

        await _dbContext.SaveChangesAsync();
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
