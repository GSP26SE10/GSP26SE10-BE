using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class ExtraChargeCatalogServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private ExtraChargeCatalogService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new ExtraChargeCatalogService(new ExtraChargeCatalogRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.ExtraChargeCatalogs.AddRange(
            new ExtraChargeCatalog
            {
                ExtraChargeCatalogId = 6301,
                ChargeType = "BROKEN_ITEM",
                Title = "Broken Plate",
                Description = "Broken plate fee",
                Unit = "item",
                UnitPrice = 25_000,
                Status = "ACTIVE"
            },
            new ExtraChargeCatalog
            {
                ExtraChargeCatalogId = 6302,
                ChargeType = "OVERTIME",
                Title = "Overtime Service",
                Description = "Overtime fee",
                Unit = "hour",
                UnitPrice = 100_000,
                Status = "INACTIVE"
            });

        _dbContext.OrderDetailExtraCharges.Add(new OrderDetailExtraCharge
        {
            OrderDetailExtraChargeId = 6601,
            ExtraChargeCatalogId = 6301,
            ChargeType = "BROKEN_ITEM",
            Title = "Broken Plate",
            Unit = "item",
            UnitPrice = 25_000,
            Quantity = 1,
            TotalAmount = 25_000,
            Status = "ACTIVE"
        });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 63 - GetAllExtraChargeCatalogsFiltered
    //Function 63 - TC1
    [TestMethod]
    public async Task GetAllFilteredAsync_WhenFilterByStatus_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllFilteredAsync(
            new ExtraChargeCatalogFilterRequest { Status = ExtraChargeCatalogStatus.Inactive },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().ExtraChargeCatalogId.Should().Be(6302);
        result.Items.First().Status.Should().Be("INACTIVE");
    }

    //Function 63 - TC2
    [TestMethod]
    public async Task GetAllFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllFilteredAsync(
            new ExtraChargeCatalogFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().ExtraChargeCatalogId.Should().Be(6302);
    }
    #endregion

    #region Function 64 - CreateExtraChargeCatalog
    //Function 64 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenRequiredFieldsMissing_ShouldFail()
    {
        var result = await _sut.CreateAsync(new ExtraChargeCatalogCreateRequest
        {
            ChargeType = " ",
            Title = " ",
            Unit = " "
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("ChargeType, Title and Unit are required.");
        result.Data.Should().BeNull();
    }

    //Function 64 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenDuplicatedByChargeTypeAndTitle_ShouldFail()
    {
        var result = await _sut.CreateAsync(new ExtraChargeCatalogCreateRequest
        {
            ChargeType = "broken_item",
            Title = "broken plate",
            Unit = "item",
            UnitPrice = 10_000
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog with the same charge type and title already exists.");
        result.Data.Should().BeNull();
    }

    //Function 64 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new ExtraChargeCatalogCreateRequest
        {
            ChargeType = "  travel_fee ",
            Title = " Travel Support ",
            Description = "  support moving ",
            Unit = " trip ",
            UnitPrice = 150_000
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Extra charge catalog created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.ChargeType.Should().Be("TRAVEL_FEE");
        result.Data.Title.Should().Be("Travel Support");
        result.Data.Unit.Should().Be("trip");
        result.Data.Status.Should().Be("ACTIVE");

        var saved = await _dbContext.ExtraChargeCatalogs.AsNoTracking()
            .FirstAsync(x => x.ExtraChargeCatalogId == result.Data.ExtraChargeCatalogId);
        saved.ChargeType.Should().Be("TRAVEL_FEE");
        saved.Title.Should().Be("Travel Support");
    }
    #endregion

    #region Function 65 - UpdateExtraChargeCatalog
    //Function 65 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenCatalogNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new ExtraChargeCatalogUpdateRequest
        {
            ChargeType = "NEW",
            Title = "New",
            Unit = "unit",
            Status = ExtraChargeCatalogStatus.Active
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog not found.");
        result.Data.Should().BeNull();
    }

    //Function 65 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenRequiredFieldsMissing_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6302, new ExtraChargeCatalogUpdateRequest
        {
            ChargeType = " ",
            Title = " ",
            Unit = " ",
            Status = ExtraChargeCatalogStatus.Active
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("ChargeType, Title, Unit and Status are required.");
        result.Data.Should().BeNull();
    }

    //Function 65 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenDuplicatedByChargeTypeAndTitle_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6302, new ExtraChargeCatalogUpdateRequest
        {
            ChargeType = "broken_item",
            Title = "broken plate",
            Unit = "hour",
            UnitPrice = 50_000,
            Status = ExtraChargeCatalogStatus.Active
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog with the same charge type and title already exists.");
        result.Data.Should().BeNull();
    }

    //Function 65 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(6302, new ExtraChargeCatalogUpdateRequest
        {
            ChargeType = " overtime ",
            Title = " Overtime Service Updated ",
            Description = " updated ",
            Unit = " hour ",
            UnitPrice = 120_000,
            Status = ExtraChargeCatalogStatus.Active
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Extra charge catalog updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.ChargeType.Should().Be("OVERTIME");
        result.Data.Title.Should().Be("Overtime Service Updated");
        result.Data.Status.Should().Be("ACTIVE");

        var saved = await _dbContext.ExtraChargeCatalogs.AsNoTracking().FirstAsync(x => x.ExtraChargeCatalogId == 6302);
        saved.ChargeType.Should().Be("OVERTIME");
        saved.Title.Should().Be("Overtime Service Updated");
        saved.Status.Should().Be("ACTIVE");
    }
    #endregion

    #region Function 66 - DeleteExtraChargeCatalog
    //Function 66 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenCatalogNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Extra charge catalog not found.");
        result.Data.Should().BeFalse();
    }

    //Function 66 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenCatalogHasRelatedData_ShouldFail()
    {
        var result = await _sut.DeleteAsync(6301);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete extra charge catalog because it is being used by order detail extra charges.");
        result.Data.Should().BeFalse();
    }

    //Function 66 - TC3
    [TestMethod]
    public async Task DeleteAsync_WhenCatalogNotInUse_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(6302);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Extra charge catalog deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.ExtraChargeCatalogs.AnyAsync(x => x.ExtraChargeCatalogId == 6302)).Should().BeFalse();
    }
    #endregion
}

