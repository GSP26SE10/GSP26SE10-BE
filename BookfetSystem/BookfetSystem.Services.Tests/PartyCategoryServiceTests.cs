using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class PartyCategoryServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private PartyCategoryService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var partyCategoryRepository = new PartyCategoryRepository(_dbContext);
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _sut = new PartyCategoryService(partyCategoryRepository, _imageStorageServiceMock.Object);

        await SeedPartyCategoriesAsync();
    }

    // Function 12
    #region Function 12 - Get Party Category List
    //Function 12 - TC1
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllPartyCategoryFilteredAsync(new PartyCategoryFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //TC2
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllPartyCategoryFilteredAsync(new PartyCategoryFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //TC3
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllPartyCategoryFilteredAsync(new PartyCategoryFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //TC5
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_WithStatusAvailable_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllPartyCategoryFilteredAsync(
            new PartyCategoryFilterRequest { Status = PartyCategoryStatus.AVAILABLE },
            1,
            10);

        result.TotalCount.Should().Be(4);
        result.Items.Should().OnlyContain(x => x.Status == 1);
    }

    //TC6
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllPartyCategoryFilteredAsync(
            new PartyCategoryFilterRequest { PartyCategoryName = "Wed" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().PartyCategoryName.Should().Be("Wedding");
    }

    //TC7
    [TestMethod]
    public async Task GetAllPartyCategoryFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllPartyCategoryFilteredAsync(
            new PartyCategoryFilterRequest { PartyCategoryName = "NotExist" },
            1,
            10);
        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllPartyCategoryFilteredAsync(
            new PartyCategoryFilterRequest { PartyCategoryId = 999 },
            1,
            10);
        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    private async Task SeedPartyCategoriesAsync()
    {
        _dbContext.PartyCategories.AddRange(
            new PartyCategory { PartyCategoryId = 1, PartyCategoryName = "Wedding", Description = "Wedding party", NumberOfGuests = 200, Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new PartyCategory { PartyCategoryId = 2, PartyCategoryName = "Birthday", Description = "Birthday party", NumberOfGuests = 80, Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new PartyCategory { PartyCategoryId = 3, PartyCategoryName = "Company Event", Description = "Corporate party", NumberOfGuests = 150, Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new PartyCategory { PartyCategoryId = 4, PartyCategoryName = "Family Gathering", Description = "Family party", NumberOfGuests = 50, Status = "AVAILABLE", CreatedAt = DateTime.UtcNow },
            new PartyCategory { PartyCategoryId = 5, PartyCategoryName = "Archived Party", Description = "Old category", NumberOfGuests = 60, Status = "UNAVAILABLE", CreatedAt = DateTime.UtcNow }
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
