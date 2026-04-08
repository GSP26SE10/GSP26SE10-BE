using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class MenuServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private MenuService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        var menuRepository = new MenuRepository(_dbContext);
        var menuCategoryRepository = new MenuCategoryRepository(_dbContext);
        var partyCategoryRepository = new PartyCategoryRepository(_dbContext);
        var partyCategoryMenuRepository = new PartyCategoryMenuRepository(_dbContext);
        var imageStorageServiceMock = new Mock<IImageStorageService>();

        _sut = new MenuService(
            menuRepository,
            menuCategoryRepository,
            partyCategoryRepository,
            partyCategoryMenuRepository,
            imageStorageServiceMock.Object);

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

        _dbContext.Menus.AddRange(
            new Menu { MenuId = 1, MenuName = "BBQ Premium", BasePrice = 300_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 2, MenuName = "Seafood Deluxe", BasePrice = 450_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 3, MenuName = "Vegetarian Set", BasePrice = 250_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 4, MenuName = "Family Combo", BasePrice = 200_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Menu { MenuId = 5, MenuName = "Birthday Party Set", BasePrice = 350_000, Status = "1", MenuCategoryId = 1, CreatedAt = DateTime.UtcNow }
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
