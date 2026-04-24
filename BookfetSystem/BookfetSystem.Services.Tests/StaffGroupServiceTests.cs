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
public class StaffGroupServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private StaffGroupService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new StaffGroupService(
            new StaffGroupRepository(_dbContext),
            new UserRepository(_dbContext));

        await SeedAssignmentOverviewDataAsync();
    }

    private async Task SeedAssignmentOverviewDataAsync()
    {
        _dbContext.Users.AddRange(
            new User
            {
                UserId = 1,
                FullName = "Customer One",
                Email = "customer@test.com",
                Phone = "0900000001",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "customer1",
                Address = "HN",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                FullName = "Leader Two",
                Email = "leader@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "leader2",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 3,
                FullName = "Staff Three",
                Email = "staff@test.com",
                Phone = "0900000003",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "staff3",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 4,
                FullName = "Leader Four",
                Email = "leader4@test.com",
                Phone = "0900000004",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "leader4",
                Address = "DN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.MenuCategories.Add(new MenuCategory
        {
            MenuCategoryId = 1,
            MenuCategoryName = "Set",
            Description = "desc",
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.Menus.Add(new Menu
        {
            MenuId = 1,
            MenuName = "Standard Menu",
            BasePrice = 200_000,
            Status = "AVAILABLE",
            ImgUrl = "[]",
            MenuCategoryId = 1,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.PartyCategories.Add(new PartyCategory
        {
            PartyCategoryId = 1,
            PartyCategoryName = "Wedding",
            Description = "desc",
            Status = "AVAILABLE",
            NumberOfGuests = 10,
            ImageUrl = string.Empty,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.StaffGroups.Add(new StaffGroup
        {
            StaffGroupId = 10,
            StaffGroupName = "Service Team",
            Status = StaffGroupStatus.ACTIVE.ToString(),
            LeaderId = 2
        });
        _dbContext.StaffGroups.Add(new StaffGroup
        {
            StaffGroupId = 11,
            StaffGroupName = "Backup Team",
            Status = StaffGroupStatus.INACTIVE.ToString(),
            LeaderId = null
        });

        _dbContext.StaffGroupMembers.Add(new StaffGroupMember
        {
            StaffGroupMemberId = 1,
            StaffGroupId = 10,
            StaffId = 3,
            Status = "ACTIVE"
        });

        var partyStart = DateTime.UtcNow.AddDays(14);
        _dbContext.Orders.Add(new Order
        {
            OrderId = 900,
            CustomerId = 1,
            Status = OrderStatus.APPROVED.ToString(),
            TotalPrice = 5_000_000,
            DepositAmount = 1_000_000,
            RemainingAmount = 0,
            CreatedAt = DateTime.UtcNow
        });

        _dbContext.OrderDetails.Add(new OrderDetail
        {
            OrderDetailId = 9001,
            OrderId = 900,
            MenuId = 1,
            PartyCategoryId = 1,
            NumberOfGuests = 50,
            Address = "HN",
            Status = OrderDetailStatus.PREPARING.ToString(),
            StaffGroupId = 10,
            StartTime = partyStart,
            EndTime = partyStart.AddHours(4)
        });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 71 - GetAllStaffGroupsFiltered
    //Function 71 - TC1
    [TestMethod]
    public async Task GetAllStaffGroupFilteredAsync_WhenFilterByStatus_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllStaffGroupFilteredAsync(
            new StaffGroupFilterRequest { Status = StaffGroupStatus.ACTIVE },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().StaffGroupId.Should().Be(10);
        result.Items.First().LeaderName.Should().Be("Leader Two");
    }

    //Function 71 - TC2
    [TestMethod]
    public async Task GetAllStaffGroupFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllStaffGroupFilteredAsync(
            new StaffGroupFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
    }
    #endregion

    #region Function 72 - CreateStaffGroup
    //Function 72 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenLeaderNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupCreateRequest
        {
            StaffGroupName = "New Team",
            LeaderId = 99999
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Leader not found.");
        result.Data.Should().BeNull();
    }

    //Function 72 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenLeaderAlreadyHasGroup_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupCreateRequest
        {
            StaffGroupName = "New Team",
            LeaderId = 2
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Leader already has a staff group.");
        result.Data.Should().BeNull();
    }

    //Function 72 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new StaffGroupCreateRequest
        {
            StaffGroupName = "  New Team  ",
            LeaderId = 4
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.LeaderId.Should().Be(4);
        result.Data.LeaderName.Should().Be("Leader Four");

        var saved = await _dbContext.StaffGroups.AsNoTracking()
            .FirstAsync(x => x.StaffGroupId == result.Data.StaffGroupId);
        saved.StaffGroupName.Should().Be("New Team");
        saved.Status.Should().Be(StaffGroupStatus.ACTIVE.ToString());
    }
    #endregion

    #region Function 73 - UpdateStaffGroup
    //Function 73 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenStaffGroupNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new StaffGroupUpdateRequest
        {
            StaffGroupName = "Updated Team",
            LeaderId = 4,
            Status = StaffGroupStatus.INACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group not found.");
        result.Data.Should().BeNull();
    }

    //Function 73 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenLeaderAlreadyInStaffGroup_ShouldFail()
    {
        var result = await _sut.UpdateAsync(11, new StaffGroupUpdateRequest
        {
            StaffGroupName = "Updated Team",
            LeaderId = 2,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Leader already in staff group");
        result.Data.Should().BeNull();
    }

    //Function 73 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenLeaderNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(11, new StaffGroupUpdateRequest
        {
            StaffGroupName = "Updated Team",
            LeaderId = 99999,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Leader not found.");
        result.Data.Should().BeNull();
    }

    //Function 73 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(11, new StaffGroupUpdateRequest
        {
            StaffGroupName = "  Updated Team  ",
            LeaderId = 4,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.LeaderId.Should().Be(4);
        result.Data.LeaderName.Should().Be("Leader Four");
        result.Data.Status.Should().Be((int)StaffGroupStatus.ACTIVE);

        var saved = await _dbContext.StaffGroups.AsNoTracking().FirstAsync(x => x.StaffGroupId == 11);
        saved.StaffGroupName.Should().Be("Updated Team");
        saved.LeaderId.Should().Be(4);
        saved.Status.Should().Be(StaffGroupStatus.ACTIVE.ToString());
    }
    #endregion

    #region Function 74 - DeleteStaffGroup
    //Function 74 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenStaffGroupNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group not found.");
        result.Data.Should().BeFalse();
    }

    //Function 74 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenStaffGroupHasOrderDetails_ShouldFail()
    {
        var result = await _sut.DeleteAsync(10);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Cannot delete staff group because it is being used in orders.");
        result.Data.Should().BeFalse();
    }

    //Function 74 - TC3
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(11);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.StaffGroups.AnyAsync(x => x.StaffGroupId == 11)).Should().BeFalse();
    }
    #endregion

    //Function 89 - TC1
    [TestMethod]
    public async Task GetAssignmentOverviewByLeaderAsync_WhenLeaderHasNoGroup_ShouldReturnNull()
    {
        var result = await _sut.GetAssignmentOverviewByLeaderAsync(leaderId: 999);

        result.Should().BeNull();
    }

    //Function 89 - TC2
    [TestMethod]
    public async Task GetAssignmentOverviewByLeaderAsync_WhenLeaderHasGroup_ShouldReturnOrdersAndMembers()
    {
        var result = await _sut.GetAssignmentOverviewByLeaderAsync(leaderId: 2);

        result.Should().NotBeNull();
        result!.StaffGroup.StaffGroupId.Should().Be(10);
        result.StaffGroup.Leader.StaffId.Should().Be(2);
        result.StaffGroup.Leader.StaffName.Should().Be("Leader Two");
        result.StaffGroup.Members.Should().ContainSingle(m => m.StaffId == 3 && m.StaffName == "Staff Three");

        result.Orders.Should().ContainSingle();
        var orderVm = result.Orders.First();
        orderVm.OrderId.Should().Be(900);
        orderVm.Customer.Name.Should().Be("Customer One");
        orderVm.Menu.Name.Should().Be("Standard Menu");
        orderVm.Party.Category.Should().Be("Wedding");
    }
}
