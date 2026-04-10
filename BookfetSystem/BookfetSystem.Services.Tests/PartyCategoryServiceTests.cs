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

    // Function 13
    #region Function 13 - Create Party Category
    //Function 13 - TC1
    [TestMethod]
    public async Task CreatePartyCategory_WhenValid_ShouldSucceed()
    {
        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = "Gala Dinner",
            Description = "Evening gala event",
            NumberOfGuests = 120
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PartyCategoryName.Should().Be("Gala Dinner");
        result.Data.Description.Should().Be("Evening gala event");
        result.Data.NumberOfGuests.Should().Be(120);
        result.Data.Status.Should().Be(1);

        (await _dbContext.PartyCategories.CountAsync()).Should().Be(6);
    }

    //Function 13 - TC2
    [TestMethod]
    public async Task CreatePartyCategory_WhenNameMissing_ShouldFail()
    {
        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = null,
            Description = "Desc",
            NumberOfGuests = 100
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("PartyCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC3
    [TestMethod]
    public async Task CreatePartyCategory_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = "No Desc Party",
            Description = "   ",
            NumberOfGuests = 70
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PartyCategoryName.Should().Be("No Desc Party");
        result.Data.Description.Should().BeNullOrEmpty();
        result.Data.Status.Should().Be(1);
    }

    //Function 13 - TC5
    [TestMethod]
    public async Task CreatePartyCategory_WhenNumberOfGuestsIsNotGreaterThanZero_ShouldFail()
    {
        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = "Invalid Guests",
            Description = "Desc",
            NumberOfGuests = 0
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("NumberOfGuests must be greater than 0.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC6
    [TestMethod]
    public async Task CreatePartyCategory_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = "  weDdiNg  ",
            Description = "Desc",
            NumberOfGuests = 100
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("PartyCategoryName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 13 - TC7
    [TestMethod]
    public async Task CreatePartyCategory_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.PartyCategory,
                null,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new PartyCategoryCreateRequest
        {
            PartyCategoryName = "With Image",
            Description = "Desc",
            NumberOfGuests = 90,
            ImageUrl = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload party category image");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 14 .
    #region Function 14 - Update Party Category
    //Function 14 - TC1
    [TestMethod]
    public async Task UpdatePartyCategory_WhenValidAllFields_ShouldSucceed()
    {
        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "Wedding Updated",
            Description = "Updated desc",
            NumberOfGuests = 250,
            Status = PartyCategoryStatus.UNAVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Party category updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PartyCategoryName.Should().Be("Wedding Updated");
        result.Data.Description.Should().Be("Updated desc");
        result.Data.NumberOfGuests.Should().Be(250);
        result.Data.Status.Should().Be(0);
    }

    //Function 14 - TC2
    [TestMethod]
    public async Task UpdatePartyCategory_WhenIdNotFound_ShouldFail()
    {
        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "Ghost",
            Description = "Desc",
            NumberOfGuests = 10,
            Status = PartyCategoryStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Party category not found.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC3
    [TestMethod]
    public async Task UpdatePartyCategory_WhenNameMissing_ShouldFail()
    {
        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "   ",
            Description = "Desc",
            NumberOfGuests = 10,
            Status = PartyCategoryStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("PartyCategoryName is required.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC4
    [TestMethod]
    public async Task UpdatePartyCategory_WhenNumberOfGuestsIsNotGreaterThanZero_ShouldFail()
    {
        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "Wedding",
            Description = "Desc",
            NumberOfGuests = 0,
            Status = PartyCategoryStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("NumberOfGuests must be greater than 0.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC5
    [TestMethod]
    public async Task UpdatePartyCategory_WhenNameAlreadyExists_ShouldFail()
    {
        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "  birthday ",
            Description = "Desc",
            NumberOfGuests = 100,
            Status = PartyCategoryStatus.AVAILABLE
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("PartyCategoryName is existed.");
        result.Data.Should().BeNull();
    }

    //Function 14 - TC6
    [TestMethod]
    public async Task UpdatePartyCategory_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.PartyCategory,
                1,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new PartyCategoryUpdateRequest
        {
            PartyCategoryName = "Wedding",
            Description = "Desc",
            NumberOfGuests = 100,
            Status = PartyCategoryStatus.AVAILABLE,
            ImageUrl = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload party category image");
        result.Data.Should().BeNull();
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

    private static IFormFile CreateFormFile(string fileName, string contentType)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImageUrl", fileName)
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
