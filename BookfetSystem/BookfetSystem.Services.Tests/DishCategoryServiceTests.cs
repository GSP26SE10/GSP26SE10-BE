using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class DishCategoryServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private DishCategoryService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var dishCategoryRepository = new DishCategoryRepository(_dbContext);
        _sut = new DishCategoryService(dishCategoryRepository);

        await SeedDishCategoriesAsync();
    }

    // Function 20 
    #region Function 20 - Get Dish Category List
    //Function 20 - TC1
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllDishCategoryFilteredAsync(new DishCategoryFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 20 - TC2
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllDishCategoryFilteredAsync(new DishCategoryFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 20 - TC3
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllDishCategoryFilteredAsync(new DishCategoryFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 20 - TC4 
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllDishCategoryFilteredAsync(
            new DishCategoryFilterRequest { DishCategoryName = "Dess" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().DishCategoryName.Should().Be("Desserts");
    }

    //Function 20 - TC5
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_WithDescription_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllDishCategoryFilteredAsync(
            new DishCategoryFilterRequest { Description = "drink" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.First().DishCategoryName.Should().Be("Beverages");
    }

    //Function 20 - TC6&7
    [TestMethod]
    public async Task GetAllDishCategoryFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllDishCategoryFilteredAsync(
            new DishCategoryFilterRequest { DishCategoryName = "NotExist" },
            1,
            10);
        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllDishCategoryFilteredAsync(
            new DishCategoryFilterRequest { DishCategoryId = 999 },
            1,
            10);
        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    // Function 21 
    #region Function 21 - Create Dish Category
    //Function 21 - TC1
    [TestMethod]
    public async Task CreateDishCategory_WhenValid_ShouldSucceed()
    {
        var request = new DishCategoryCreateRequest
        {
            DishCategoryName = "Salads",
            Description = "Fresh salads"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish category created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishCategoryName.Should().Be("Salads");
        result.Data.Description.Should().Be("Fresh salads");

        (await _dbContext.DishCategories.CountAsync()).Should().Be(6);
    }

    //Function 21 - TC2
    [TestMethod]
    public async Task CreateDishCategory_WhenNameMissing_ShouldFail()
    {
        var request = new DishCategoryCreateRequest
        {
            DishCategoryName = null,
            Description = "Desc"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 21 - TC3
    [TestMethod]
    public async Task CreateDishCategory_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new DishCategoryCreateRequest
        {
            DishCategoryName = "Sides",
            Description = "   "
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DishCategoryName.Should().Be("Sides");
        result.Data.Description.Should().BeNullOrEmpty();
    }

    //Function 21 - TC4
    [TestMethod]
    public async Task CreateDishCategory_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new DishCategoryCreateRequest
        {
            DishCategoryName = "  Appetizers  ",
            Description = "Dup"
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategoryName is existed.");
        result.Data.Should().BeNull();
    }
    #endregion

    #region Function 22 - Update Dish Category
    //Function 22 - TC1
    [TestMethod]
    public async Task UpdateDishCategory_WhenValidAllFields_ShouldSucceed()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "Appetizers Updated",
            Description = "Updated description"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Dish category updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.DishCategoryName.Should().Be("Appetizers Updated");
        result.Data.Description.Should().Be("Updated description");
    }

    //Function 22 - TC2
    [TestMethod]
    public async Task UpdateDishCategory_WhenNameEmpty_ShouldFail()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "   ",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 22 - TC3
    [TestMethod]
    public async Task UpdateDishCategory_WhenIdNotFound_ShouldFail()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "Ghost",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
        result.Data.Should().BeNull();
    }

    //Function 22 - TC4
    [TestMethod]
    public async Task UpdateDishCategory_WhenIdIsZero_ShouldFail()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "X",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(0, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
        result.Data.Should().BeNull();
    }

    //Function 22 - TC5
    [TestMethod]
    public async Task UpdateDishCategory_WhenIdIsNegative_ShouldFail()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "X",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(-1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
        result.Data.Should().BeNull();
    }

    //Function 22 - TC6
    [TestMethod]
    public async Task UpdateDishCategory_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new DishCategoryUpdateRequest
        {
            DishCategoryName = "Desserts",
            Description = "Desc"
        };

        var result = await _sut.UpdateAsync(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("DishCategoryName is existed.");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 23 
    #region Function 23 - Delete Dish Category
    //Function 23 - TC1
    [TestMethod]
    public async Task DeleteDishCategory_WhenValidId_ShouldSucceed()
    {
        var result = await _sut.DeleteAsync(5);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Dish category deleted successfully.");

        (await _dbContext.DishCategories.FindAsync(5)).Should().BeNull();
    }

    //Function 23 - TC2
    [TestMethod]
    public async Task DeleteDishCategory_WhenIdNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(999);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
    }

    //Function 23 - TC3
    [TestMethod]
    public async Task DeleteDishCategory_WhenIdIsZero_ShouldFail()
    {
        var result = await _sut.DeleteAsync(0);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
    }

    //Function 23 - TC4
    [TestMethod]
    public async Task DeleteDishCategory_WhenIdIsNegative_ShouldFail()
    {
        var result = await _sut.DeleteAsync(-1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Dish category not found.");
    }

    //Function 23 - TC5
    [TestMethod]
    public async Task DeleteDishCategory_WhenDeleteSameIdTwice_SecondShouldFail()
    {
        var first = await _sut.DeleteAsync(4);
        first.Success.Should().BeTrue();

        var second = await _sut.DeleteAsync(4);

        second.Success.Should().BeFalse();
        second.Data.Should().BeFalse();
        second.Message.Should().Be("Dish category not found.");
    }

    //Function 23 - TC6
    [TestMethod]
    public async Task DeleteDishCategory_WhenRelatedDataExists_ShouldFail()
    {
        _dbContext.Dishes.Add(new Dish
        {
            DishId = 9001,
            DishName = "Test dish",
            Price = 10_000,
            Description = "",
            Note = "",
            Img = "",
            Status = "AVAILABLE",
            DishCategoryId = 1
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteAsync(1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Cannot delete dish category because it is being used in related data.");
    }
    #endregion

    private async Task SeedDishCategoriesAsync()
    {
        _dbContext.DishCategories.AddRange(
            new DishCategory { DishCategoryId = 1, DishCategoryName = "Appetizers", Description = "Small plates" },
            new DishCategory { DishCategoryId = 2, DishCategoryName = "Main Course", Description = "Hearty main dishes" },
            new DishCategory { DishCategoryId = 3, DishCategoryName = "Desserts", Description = "Sweet desserts" },
            new DishCategory { DishCategoryId = 4, DishCategoryName = "Beverages", Description = "Cold and hot drinks" },
            new DishCategory { DishCategoryId = 5, DishCategoryName = "Archived", Description = "Old category" }
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
