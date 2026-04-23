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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class UserServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private UserService _sut = null!;
    private Mock<IImageStorageService> _imageStorageServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn.test/user/avatar.jpg");

        _sut = new UserService(new UserRepository(_dbContext), _imageStorageServiceMock.Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Roles.AddRange(
            new Role { RoleId = 1, RoleName = "Owner" },
            new Role { RoleId = 4, RoleName = "Customer" });

        _dbContext.Users.AddRange(
            new User
            {
                UserId = 5201,
                UserName = "owner1",
                PasswordHash = "hash",
                FullName = "Owner One",
                Email = "owner1@test.com",
                Phone = "0900000001",
                Avatar = string.Empty,
                Address = "HN",
                Status = UserStatus.ACTIVE.ToString(),
                RoleId = 1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new User
            {
                UserId = 5202,
                UserName = "customer1",
                PasswordHash = "hash",
                FullName = "Customer One",
                Email = "customer1@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Address = "HCM",
                Status = UserStatus.INACTIVE.ToString(),
                RoleId = 4,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await _dbContext.SaveChangesAsync();
    }

    private static UserCreateRequest BuildValidCreateRequest() => new()
    {
        UserName = "newuser",
        Password = "123456",
        FullName = "New User",
        Email = "newuser@test.com",
        Address = "DN",
        Phone = "0900000009",
        RoleId = 4
    };

    #region Function 52 - GetAllUsersFiltered
    //Function 52 - TC1
    [TestMethod]
    public async Task GetAllUserFilteredAsync_WhenFilterByStatus_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllUserFilteredAsync(
            new UserFilterRequest { Status = UserStatus.ACTIVE },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().UserName.Should().Be("owner1");
        result.Items.First().Status.Should().Be((int)UserStatus.ACTIVE);
    }

    //Function 52 - TC2
    [TestMethod]
    public async Task GetAllUserFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllUserFilteredAsync(new UserFilterRequest(), page: 2, pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().UserName.Should().Be("owner1");
    }
    #endregion

    #region Function 53 - CreateUser
    //Function 53 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenRoleNotExist_ShouldFail()
    {
        var request = BuildValidCreateRequest();
        request.RoleId = 999;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Role not exist, try correct input role id.");
        result.Data.Should().BeNull();
    }

    //Function 53 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenUsernameExisted_ShouldFail()
    {
        var request = BuildValidCreateRequest();
        request.UserName = "owner1";

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Username is existed.");
        result.Data.Should().BeNull();
    }

    //Function 53 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenEmailExisted_ShouldFail()
    {
        var request = BuildValidCreateRequest();
        request.Email = "owner1@test.com";

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email is existed.");
        result.Data.Should().BeNull();
    }

    //Function 53 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenAvatarUploadFails_ShouldFail()
    {
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("upload failed"));

        var request = BuildValidCreateRequest();
        request.AvatarFile = new Mock<IFormFile>().Object;

        var result = await _sut.CreateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload avatar: upload failed");
        result.Data.Should().BeNull();
    }

    //Function 53 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(BuildValidCreateRequest());

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.UserName.Should().Be("newuser");
        result.Data.Status.Should().Be((int)UserStatus.ACTIVE);
        result.Data.RoleName.Should().Be("Customer");

        var saved = await _dbContext.Users.AsNoTracking().FirstAsync(x => x.UserName == "newuser");
        saved.Email.Should().Be("newuser@test.com");
        saved.Status.Should().Be(UserStatus.ACTIVE.ToString());
        saved.PasswordHash.Should().NotBe("123456");
    }
    #endregion

    #region Function 54 - UpdateUser
    //Function 54 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenUserNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new UserUpdateRequest { FullName = "Any" });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Data.Should().BeNull();
    }

    //Function 54 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenEmailExisted_ShouldFail()
    {
        _dbContext.Users.Add(new User
        {
            UserId = 5203,
            UserName = "customer2",
            PasswordHash = "hash",
            FullName = "Customer Two",
            Email = "customer2@test.com",
            Phone = "0900000003",
            Avatar = string.Empty,
            Address = "HN",
            Status = UserStatus.ACTIVE.ToString(),
            RoleId = 4,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.UpdateAsync(5201, new UserUpdateRequest
        {
            Email = "customer2@test.com"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Email is existed.");
        result.Data.Should().BeNull();
    }

    //Function 54 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenAvatarUploadFails_ShouldFail()
    {
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("cannot upload"));

        var result = await _sut.UpdateAsync(5201, new UserUpdateRequest
        {
            FullName = "Owner Updated",
            AvatarFile = new Mock<IFormFile>().Object
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload avatar: cannot upload");
        result.Data.Should().BeNull();
    }

    //Function 54 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(5201, new UserUpdateRequest
        {
            FullName = "Owner Updated",
            Address = "Da Nang",
            Email = "owner1new@test.com",
            Phone = "0909999999",
            Status = UserStatus.INACTIVE
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.FullName.Should().Be("Owner Updated");
        result.Data.Email.Should().Be("owner1new@test.com");
        result.Data.Status.Should().Be((int)UserStatus.INACTIVE);

        var saved = await _dbContext.Users.AsNoTracking().FirstAsync(x => x.UserId == 5201);
        saved.FullName.Should().Be("Owner Updated");
        saved.Address.Should().Be("Da Nang");
        saved.Phone.Should().Be("0909999999");
        saved.Status.Should().Be(UserStatus.INACTIVE.ToString());
    }
    #endregion

    #region Function 55 - DeleteUser
    //Function 55 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenUserNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Data.Should().BeFalse();
    }

    //Function 55 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(5202);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Account deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.Users.AnyAsync(x => x.UserId == 5202)).Should().BeFalse();
    }
    #endregion
}

