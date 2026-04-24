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
public class StaffGroupMemberServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private StaffGroupMemberService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new StaffGroupMemberService(
            new StaffGroupMemberRepository(_dbContext),
            new StaffGroupRepository(_dbContext),
            new UserRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Roles.AddRange(
            new Role { RoleId = 2, RoleName = "GROUP_LEADER" },
            new Role { RoleId = 3, RoleName = "STAFF" },
            new Role { RoleId = 4, RoleName = "CUSTOMER" });

        _dbContext.Users.AddRange(
            new User
            {
                UserId = 1,
                FullName = "Leader One",
                Email = "leader1@test.com",
                Phone = "0900000001",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "leader1",
                Address = "HN",
                RoleId = 2,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                FullName = "Staff Two",
                Email = "staff2@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "staff2",
                Address = "HN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 3,
                FullName = "Staff Three",
                Email = "staff3@test.com",
                Phone = "0900000003",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "staff3",
                Address = "DN",
                RoleId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 4,
                FullName = "Customer Four",
                Email = "customer4@test.com",
                Phone = "0900000004",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "customer4",
                Address = "HCM",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.StaffGroups.AddRange(
            new StaffGroup
            {
                StaffGroupId = 75,
                StaffGroupName = "Team A",
                Status = StaffGroupStatus.ACTIVE.ToString(),
                LeaderId = 1
            },
            new StaffGroup
            {
                StaffGroupId = 76,
                StaffGroupName = "Team B",
                Status = StaffGroupStatus.ACTIVE.ToString(),
                LeaderId = null
            });

        _dbContext.StaffGroupMembers.AddRange(
            new StaffGroupMember
            {
                StaffGroupMemberId = 7501,
                StaffGroupId = 75,
                StaffId = 2,
                Status = StaffGroupStatus.ACTIVE.ToString()
            },
            new StaffGroupMember
            {
                StaffGroupMemberId = 7502,
                StaffGroupId = 76,
                StaffId = 3,
                Status = StaffGroupStatus.INACTIVE.ToString()
            });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 75 - GetAllStaffGroupMembersFiltered
    //Function 75 - TC1
    [TestMethod]
    public async Task GetAllStaffGroupMemberFilteredAsync_WhenFilterByStaffGroup_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllStaffGroupMemberFilteredAsync(
            new StaffGroupMemberFilterRequest { StaffGroupId = 75 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().StaffGroupMemberId.Should().Be(7501);
        result.Items.First().StaffName.Should().Be("Staff Two");
        result.Items.First().StaffGroupName.Should().Be("Team A");
    }

    //Function 75 - TC2
    [TestMethod]
    public async Task GetAllStaffGroupMemberFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllStaffGroupMemberFilteredAsync(
            new StaffGroupMemberFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().StaffGroupMemberId.Should().Be(7502);
    }
    #endregion

    #region Function 76 - CreateStaffGroupMember
    //Function 76 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenStaffGroupNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 99999,
            StaffId = 2
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group not found.");
        result.Data.Should().BeNull();
    }

    //Function 76 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenStaffNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 75,
            StaffId = 99999
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff not found.");
        result.Data.Should().BeNull();
    }

    //Function 76 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenUserRoleInvalid_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 75,
            StaffId = 4
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User must have STAFF or GROUP_LEADER role to be added to staff group.");
        result.Data.Should().BeNull();
    }

    //Function 76 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenAlreadyInSameGroup_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 75,
            StaffId = 2
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff is already a member of this group.");
        result.Data.Should().BeNull();
    }

    //Function 76 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenStaffAlreadyInAnotherGroup_ShouldFail()
    {
        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 75,
            StaffId = 3
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff can only belong to one group. Remove them from current group first.");
        result.Data.Should().BeNull();
    }

    //Function 76 - TC6
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        _dbContext.Users.Add(new User
        {
            UserId = 5,
            FullName = "Staff Five",
            Email = "staff5@test.com",
            Phone = "0900000005",
            Avatar = string.Empty,
            Status = "ACTIVE",
            PasswordHash = "hash",
            UserName = "staff5",
            Address = "HN",
            RoleId = 3,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CreateAsync(new StaffGroupMemberCreateRequest
        {
            StaffGroupId = 75,
            StaffId = 5
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group member created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.StaffGroupId.Should().Be(75);
        result.Data.StaffId.Should().Be(5);
        result.Data.StaffName.Should().Be("Staff Five");
        result.Data.StaffGroupName.Should().Be("Team A");
    }
    #endregion

    #region Function 77 - UpdateStaffGroupMember
    //Function 77 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenStaffGroupMemberNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 2,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group member not found.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenStaffGroupNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(7501, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 99999,
            StaffId = 2,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group not found.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenStaffNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(7501, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 99999,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff not found.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenStaffRoleInvalid_ShouldFail()
    {
        var result = await _sut.UpdateAsync(7501, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 4,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User must have STAFF or GROUP_LEADER role to be added to staff group.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenDuplicateInSameGroup_ShouldFail()
    {
        _dbContext.Users.Add(new User
        {
            UserId = 5,
            FullName = "Staff Five",
            Email = "staff5@test.com",
            Phone = "0900000005",
            Avatar = string.Empty,
            Status = "ACTIVE",
            PasswordHash = "hash",
            UserName = "staff5",
            Address = "HN",
            RoleId = 3,
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.StaffGroupMembers.Add(new StaffGroupMember
        {
            StaffGroupMemberId = 7503,
            StaffGroupId = 75,
            StaffId = 5,
            Status = StaffGroupStatus.ACTIVE.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7501, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 5,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff is already a member of this group.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC6
    [TestMethod]
    public async Task UpdateAsync_WhenStaffAlreadyInAnyGroup_ShouldFail()
    {
        _dbContext.Users.Add(new User
        {
            UserId = 6,
            FullName = "Staff Six",
            Email = "staff6@test.com",
            Phone = "0900000006",
            Avatar = string.Empty,
            Status = "ACTIVE",
            PasswordHash = "hash",
            UserName = "staff6",
            Address = "HN",
            RoleId = 3,
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.StaffGroupMembers.Add(new StaffGroupMember
        {
            StaffGroupMemberId = 7601,
            StaffGroupId = 76,
            StaffId = 6,
            Status = StaffGroupStatus.ACTIVE.ToString()
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(7501, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 6,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff can only belong to one group. Remove them from current group first.");
        result.Data.Should().BeNull();
    }

    //Function 77 - TC7
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(7502, new StaffGroupMemberUpdateRequest
        {
            StaffGroupId = 75,
            StaffId = 3,
            Status = StaffGroupStatus.ACTIVE
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group member updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.StaffGroupId.Should().Be(75);
        result.Data.StaffId.Should().Be(3);
        result.Data.Status.Should().Be((int)StaffGroupStatus.ACTIVE);

        var saved = await _dbContext.StaffGroupMembers.AsNoTracking().FirstAsync(x => x.StaffGroupMemberId == 7502);
        saved.StaffGroupId.Should().Be(75);
        saved.StaffId.Should().Be(3);
        saved.Status.Should().Be(StaffGroupStatus.ACTIVE.ToString());
    }
    #endregion

    #region Function 78 - DeleteStaffGroupMember
    //Function 78 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenStaffGroupMemberNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Staff group member not found.");
        result.Data.Should().BeFalse();
    }

    //Function 78 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(7501);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Staff group member deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.StaffGroupMembers.AnyAsync(x => x.StaffGroupMemberId == 7501)).Should().BeFalse();
    }
    #endregion
}

