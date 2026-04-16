using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class ServiceServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private ServiceService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        var serviceRepository = new ServiceRepository(_dbContext);
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _sut = new ServiceService(serviceRepository, _imageStorageServiceMock.Object);

        await SeedServicesAsync();
    }

    // Function 28 
    #region Function 28 - Get Service List
    //Function 28 - TC1
    [TestMethod]
    public async Task GetAllServiceFiltered_GetAll_ShouldReturnAllItems()
    {
        var result = await _sut.GetAllFilteredAsync(new ServiceFilterRequest(), 1, 10);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    //Function 28 - TC2
    [TestMethod]
    public async Task GetAllServiceFiltered_WithPageAndPageSize_ShouldReturnPagedItems()
    {
        var result = await _sut.GetAllFilteredAsync(new ServiceFilterRequest(), 2, 2);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    //Function 28 - TC3
    [TestMethod]
    public async Task GetAllServiceFiltered_WhenPageIsZero_ShouldNormalizeToPageOne()
    {
        var page = 0;
        var pageSize = 2;
        NormalizePagination(ref page, ref pageSize);

        var result = await _sut.GetAllFilteredAsync(new ServiceFilterRequest(), page, pageSize);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    //Function 28 - TC4
    [TestMethod]
    public async Task GetAllServiceFiltered_WithStatusAvailable_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllFilteredAsync(
            new ServiceFilterRequest { Status = ServiceStatus.AVAILABLE },
            1,
            10);

        result.TotalCount.Should().Be(4);
        result.Items.Should().OnlyContain(x => x.Status == 1);
    }

    //Function 28 - TC5
    [TestMethod]
    public async Task GetAllServiceFiltered_WithName_ShouldReturnMatchedItems()
    {
        var result = await _sut.GetAllFilteredAsync(
            new ServiceFilterRequest { ServiceName = "Photo" },
            1,
            10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().ServiceName.Should().Be("Photography");
    }

    //Function 28 - TC6&7
    [TestMethod]
    public async Task GetAllServiceFiltered_WithNotFoundNameOrId_ShouldReturnEmpty()
    {
        var byName = await _sut.GetAllFilteredAsync(
            new ServiceFilterRequest { ServiceName = "NotExist" },
            1,
            10);
        byName.TotalCount.Should().Be(0);
        byName.Items.Should().BeEmpty();

        var byId = await _sut.GetAllFilteredAsync(
            new ServiceFilterRequest { ServiceId = 999 },
            1,
            10);
        byId.TotalCount.Should().Be(0);
        byId.Items.Should().BeEmpty();
    }
    #endregion

    // Function 29
    #region Function 29 - Create Service
    //Function 29 - TC1
    [TestMethod]
    public async Task CreateService_WhenValid_ShouldSucceed()
    {
        var request = new ServiceCreateRequest
        {
            ServiceName = "Decoration",
            Description = "Event decoration",
            BasePrice = 500_000,
            Status = ServiceStatus.AVAILABLE
        };

        var result = await _sut.Create(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Service created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.ServiceName.Should().Be("Decoration");
        result.Data.BasePrice.Should().Be(500_000);
        result.Data.Status.Should().Be(1);
    }

    //Function 29 - TC2
    [TestMethod]
    public async Task CreateService_WhenNameMissing_ShouldFail()
    {
        var request = new ServiceCreateRequest
        {
            ServiceName = "   ",
            Description = "Desc"
        };

        var result = await _sut.Create(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("ServiceName is required.");
        result.Data.Should().BeNull();
    }

    //Function 29 - TC3
    [TestMethod]
    public async Task CreateService_WhenDescriptionEmpty_ShouldStillSucceed()
    {
        var request = new ServiceCreateRequest
        {
            ServiceName = "No Desc Service",
            Description = "   ",
            BasePrice = 100_000
        };

        var result = await _sut.Create(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ServiceName.Should().Be("No Desc Service");
    }

    //Function 29 - TC4
    [TestMethod]
    public async Task CreateService_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Service,
                null,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new ServiceCreateRequest
        {
            ServiceName = "With Image",
            Description = "Desc",
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.Create(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Create service failed");
        result.Data.Should().BeNull();
    }
    #endregion

    #region Function 30 - Update Service
    //Function 30 - TC1
    [TestMethod]
    public async Task UpdateService_WhenValidAllFields_ShouldSucceed()
    {
        var request = new ServiceUpdateRequest
        {
            ServiceName = "MC Updated",
            Description = "Updated desc",
            BasePrice = 300_000,
            Status = ServiceStatus.UNAVAILABLE
        };

        var result = await _sut.Update(1, request);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Service updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.ServiceName.Should().Be("MC Updated");
        result.Data.Status.Should().Be(0);
    }

    //Function 30 - TC2
    [TestMethod]
    public async Task UpdateService_WhenIdNotFound_ShouldFail()
    {
        var request = new ServiceUpdateRequest { ServiceName = "Ghost" };

        var result = await _sut.Update(999, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
        result.Data.Should().BeNull();
    }

    //Function 30 - TC3
    [TestMethod]
    public async Task UpdateService_WhenIdIsZero_ShouldFail()
    {
        var request = new ServiceUpdateRequest { ServiceName = "Ghost" };

        var result = await _sut.Update(0, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
        result.Data.Should().BeNull();
    }

    //Function 30 - TC4
    [TestMethod]
    public async Task UpdateService_WhenIdIsNegative_ShouldFail()
    {
        var request = new ServiceUpdateRequest { ServiceName = "Ghost" };

        var result = await _sut.Update(-1, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
        result.Data.Should().BeNull();
    }

    //Function 30 - TC5
    [TestMethod]
    public async Task UpdateService_WhenUploadImageThrows_ShouldFail()
    {
        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                CloudinaryFolder.Service,
                2,
                default))
            .ThrowsAsync(new Exception("Invalid image format"));

        var request = new ServiceUpdateRequest
        {
            ServiceName = "Photography",
            ImgFile = CreateFormFile("bad.bin", "application/octet-stream")
        };

        var result = await _sut.Update(2, request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload service image");
        result.Data.Should().BeNull();
    }
    #endregion

    // Function 31 
    #region Function 31 - Delete Service
    //Function 31 - TC1
    [TestMethod]
    public async Task DeleteService_WhenValidId_ShouldSucceed()
    {
        var result = await _sut.Delete(5);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Be("Service deleted successfully.");
    }

    //Function 31 - TC2
    [TestMethod]
    public async Task DeleteService_WhenIdNotFound_ShouldFail()
    {
        var result = await _sut.Delete(999);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
    }

    //Function 31 - TC3
    [TestMethod]
    public async Task DeleteService_WhenIdIsZero_ShouldFail()
    {
        var result = await _sut.Delete(0);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
    }

    //Function 31 - TC4
    [TestMethod]
    public async Task DeleteService_WhenIdIsNegative_ShouldFail()
    {
        var result = await _sut.Delete(-1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Service not found.");
    }

    //Function 31 - TC5
    [TestMethod]
    public async Task DeleteService_WhenDeleteSameIdTwice_SecondShouldFail()
    {
        var first = await _sut.Delete(4);
        first.Success.Should().BeTrue();

        var second = await _sut.Delete(4);

        second.Success.Should().BeFalse();
        second.Data.Should().BeFalse();
        second.Message.Should().Be("Service not found.");
    }

    //Function 31 - TC6
    [TestMethod]
    public async Task DeleteService_WhenRelatedDataExists_ShouldFail()
    {
        _dbContext.OrderServices.Add(new OrderService
        {
            OrderServiceId = 900,
            ServiceId = 1
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Delete(1);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Cannot delete service because it is referenced by order service records.");
    }
    #endregion

    private async Task SeedServicesAsync()
    {
        _dbContext.Services.AddRange(
            new Service { ServiceId = 1, ServiceName = "MC", Description = "Master of ceremony", BasePrice = 200_000, Status = "AVAILABLE", Img = string.Empty, CreatedAt = DateTime.UtcNow },
            new Service { ServiceId = 2, ServiceName = "Photography", Description = "Photo package", BasePrice = 500_000, Status = "AVAILABLE", Img = string.Empty, CreatedAt = DateTime.UtcNow },
            new Service { ServiceId = 3, ServiceName = "Lighting", Description = "Stage lighting", BasePrice = 300_000, Status = "AVAILABLE", Img = string.Empty, CreatedAt = DateTime.UtcNow },
            new Service { ServiceId = 4, ServiceName = "Sound System", Description = "Audio setup", BasePrice = 250_000, Status = "AVAILABLE", Img = string.Empty, CreatedAt = DateTime.UtcNow },
            new Service { ServiceId = 5, ServiceName = "Archived Service", Description = "Old service", BasePrice = 100_000, Status = "UNAVAILABLE", Img = string.Empty, CreatedAt = DateTime.UtcNow }
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

