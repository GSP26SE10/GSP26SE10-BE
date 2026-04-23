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
public class PartyCategoryMenuServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private PartyCategoryMenuService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new PartyCategoryMenuService(
            new PartyCategoryMenuRepository(_dbContext),
            new PartyCategoryRepository(_dbContext),
            new MenuRepository(_dbContext));

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
                BasePrice = 300_000,
                Status = "AVAILABLE",
                ImgUrl = "[]",
                MenuCategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.PartyCategories.AddRange(
            new PartyCategory
            {
                PartyCategoryId = 1,
                PartyCategoryName = "Wedding",
                Description = "desc",
                Status = "AVAILABLE",
                NumberOfGuests = 10,
                ImageUrl = string.Empty,
                CreatedAt = DateTime.UtcNow
            },
            new PartyCategory
            {
                PartyCategoryId = 2,
                PartyCategoryName = "Birthday",
                Description = "desc",
                Status = "AVAILABLE",
                NumberOfGuests = 20,
                ImageUrl = string.Empty,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.PartyCategoryMenus.AddRange(
            new PartyCategoryMenu { PartyCategoryMenuId = 4401, PartyCategoryId = 1, MenuId = 1 },
            new PartyCategoryMenu { PartyCategoryMenuId = 4402, PartyCategoryId = 1, MenuId = 2 },
            new PartyCategoryMenu { PartyCategoryMenuId = 4403, PartyCategoryId = 2, MenuId = 2 });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 44 - GetAllPartyCategoryMenusFiltered
    //Function 44 - TC1
    [TestMethod]
    public async Task GetAllPartyCategoryMenuFilteredAsync_WhenFilterByPartyCategory_ShouldReturnMatched()
    {
        var result = await _sut.GetAllPartyCategoryMenuFilteredAsync(
            new PartyCategoryMenuFilterRequest { PartyCategoryId = 1 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.PartyCategoryId == 1);
        result.Items.Select(x => x.PartyCategoryName).Distinct().Should().ContainSingle().Which.Should().Be("Wedding");
    }

    //Function 44 - TC2
    [TestMethod]
    public async Task GetAllPartyCategoryMenuFilteredAsync_WhenPaged_ShouldReturnCorrectPage()
    {
        var result = await _sut.GetAllPartyCategoryMenuFilteredAsync(
            new PartyCategoryMenuFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().PartyCategoryMenuId.Should().Be(4402);
    }
    #endregion

    #region Function 45 - CreatePartyCategoryMenu
    //Function 45 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenPartyCategoryNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PartyCategoryMenuCreateRequest { PartyCategoryId = 999, MenuId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Party category not found.");
        result.Data.Should().BeNull();
    }

    //Function 45 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenMenuNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PartyCategoryMenuCreateRequest { PartyCategoryId = 1, MenuId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
        result.Data.Should().BeNull();
    }

    //Function 45 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenDuplicate_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PartyCategoryMenuCreateRequest { PartyCategoryId = 1, MenuId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This menu already exists in the selected party category.");
        result.Data.Should().BeNull();
    }

    //Function 45 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new PartyCategoryMenuCreateRequest { PartyCategoryId = 2, MenuId = 1 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category menu created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PartyCategoryId.Should().Be(2);
        result.Data.MenuId.Should().Be(1);
        result.Data.PartyCategoryName.Should().Be("Birthday");
        result.Data.MenuName.Should().Be("Standard Menu");
    }
    #endregion

    #region Function 46 - UpdatePartyCategoryMenu
    //Function 46 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenPartyCategoryMenuNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new PartyCategoryMenuUpdateRequest { PartyCategoryId = 1, MenuId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Party category menu not found.");
        result.Data.Should().BeNull();
    }

    //Function 46 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenPartyCategoryNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(4401, new PartyCategoryMenuUpdateRequest { PartyCategoryId = 999, MenuId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Party category not found.");
        result.Data.Should().BeNull();
    }

    //Function 46 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenMenuNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(4401, new PartyCategoryMenuUpdateRequest { PartyCategoryId = 1, MenuId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found.");
        result.Data.Should().BeNull();
    }

    //Function 46 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenDuplicate_ShouldFail()
    {
        var result = await _sut.UpdateAsync(4402, new PartyCategoryMenuUpdateRequest { PartyCategoryId = 1, MenuId = 1 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This menu already exists in the selected party category.");
        result.Data.Should().BeNull();
    }

    //Function 46 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(4403, new PartyCategoryMenuUpdateRequest { PartyCategoryId = 2, MenuId = 2 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category menu updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PartyCategoryId.Should().Be(2);

        var saved = await _dbContext.PartyCategoryMenus.AsNoTracking().FirstAsync(x => x.PartyCategoryMenuId == 4403);
        saved.PartyCategoryId.Should().Be(2);
        saved.MenuId.Should().Be(2);
    }
    #endregion

    #region Function 47 - DeletePartyCategoryMenu
    //Function 47 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Party category menu not found.");
        result.Data.Should().BeFalse();
    }

    //Function 47 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(4402);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category menu deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.PartyCategoryMenus.AnyAsync(x => x.PartyCategoryMenuId == 4402)).Should().BeFalse();
    }
    #endregion
}

