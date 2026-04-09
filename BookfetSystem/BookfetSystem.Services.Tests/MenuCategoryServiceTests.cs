using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
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
        TypeAdapterConfig.GlobalSettings.NewConfig<MenuCategory, MenuCategoryResponse>()
            .Map(dest => dest.Status, src => ParseNullableInt(src.Status));

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

    private async Task SeedMenuCategoriesAsync()
    {
        _dbContext.MenuCategories.AddRange(
            new MenuCategory { MenuCategoryId = 1, MenuCategoryName = "BBQ Set", Description = "BBQ party set", Status = "1", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 2, MenuCategoryName = "Seafood Set", Description = "Seafood party set", Status = "1", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 3, MenuCategoryName = "Vegetarian Set", Description = "Vegetarian party set", Status = "1", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 4, MenuCategoryName = "Family Set", Description = "Family set", Status = "1", CreatedAt = DateTime.UtcNow },
            new MenuCategory { MenuCategoryId = 5, MenuCategoryName = "Archived Set", Description = "Old set", Status = "0", CreatedAt = DateTime.UtcNow }
        );

        await _dbContext.SaveChangesAsync();
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
