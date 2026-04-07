using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class AuthenticationServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private UserRepository _userRepository = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private AuthenticationService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _userRepository = new UserRepository(_dbContext);
        _emailServiceMock = new Mock<IEmailService>();
        _cacheMock = new Mock<ICache>();

        var configData = new Dictionary<string, string?>
        {
            ["JwtConfig:Issuer"] = "bookfet-test",
            ["JwtConfig:Audience"] = "bookfet-test-client",
            ["JwtConfig:Key"] = "this_is_a_test_jwt_key_with_length_gt_32",
            ["JwtConfig:ExpireMinutes"] = "60",
            ["Verification:VerifyCodeChars"] = "0123456789",
            ["Verification:VerifyCodeLength"] = "6"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new AuthenticationService(
            _userRepository,
            _emailServiceMock.Object,
            _cacheMock.Object,
            configuration);
    }

    [TestMethod]
    public async Task Register_WhenValidRequest_ShouldCreateInactiveUserAndSendVerificationEmail()
    {
        var request = new RegisterRequest
        {
            UserName = "newuser",
            Password = "123456",
            FullName = "New User",
            Email = "newuser@test.com",
            Phone = "0900000000",
            Address = "HCM"
        };

        var result = await _sut.Register(request);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");

        var createdUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        createdUser.Should().NotBeNull();
        createdUser!.Status.Should().Be("INACTIVE");
        createdUser.RoleId.Should().Be(4);
        BCrypt.Net.BCrypt.Verify(request.Password, createdUser.PasswordHash).Should().BeTrue();

        _cacheMock.Verify(
            c => c.Set(
                $"verify:{request.Email}",
                It.IsAny<string>(),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(2))),
            Times.Once);

        _emailServiceMock.Verify(
            e => e.SendAsync(
                request.Email,
                It.IsAny<string>(),
                It.Is<string>(html => html.Contains("Mã xác thực")),
                null),
            Times.Once);
    }

    [TestMethod]
    public async Task Login_WhenCredentialsAreValidAndUserIsActive_ShouldReturnAccessToken()
    {
        var password = "123456";
        _dbContext.Users.Add(new User
        {
            UserId = 100,
            UserName = "active-user",
            Email = "active@test.com",
            FullName = "Active User",
            Status = "ACTIVE",
            RoleId = 4,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            UserNameOrEmail = "active@test.com",
            Password = password
        };

        var result = await _sut.Login(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("active@test.com");
        result.Data.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Message.Should().Be("Login successful");
    }

    [TestMethod]
    public async Task VerifyEmail_WhenCodeMatches_ShouldActivateUserAndClearCache()
    {
        var email = "inactive@test.com";
        _dbContext.Users.Add(new User
        {
            UserId = 200,
            UserName = "inactive-user",
            Email = email,
            FullName = "Inactive User",
            Status = "INACTIVE",
            RoleId = 4,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.Get($"verify:{email}")).Returns("ABC123");

        var request = new VerifyEmailRequest
        {
            Email = email,
            Code = "ABC123"
        };

        var result = await _sut.VerifyEmail(request);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Message.Should().Contain("Email verified successfully");

        var updatedUser = await _dbContext.Users.FirstAsync(u => u.Email == email);
        updatedUser.Status.Should().Be("ACTIVE");

        _cacheMock.Verify(c => c.Remove($"verify:{email}"), Times.Once);
    }

    [TestMethod]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        _dbContext.Users.Add(new User
        {
            UserName = "existing-user",
            Email = "existing@test.com",
            FullName = "Existing User",
            Status = "ACTIVE",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest
        {
            UserName = "newuser",
            Password = "123456",
            FullName = "New User",
            Email = "existing@test.com"
        };

        var result = await _sut.Register(request);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Email is already registered.");
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.Never);
    }

    [TestMethod]
    public async Task Register_WhenUsernameAlreadyExists_ShouldReturnFailure()
    {
        _dbContext.Users.Add(new User
        {
            UserName = "duplicated-user",
            Email = "another@test.com",
            FullName = "Existing User",
            Status = "ACTIVE",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest
        {
            UserName = "duplicated-user",
            Password = "123456",
            FullName = "New User",
            Email = "new-email@test.com"
        };

        var result = await _sut.Register(request);

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Username already exists.");
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null), Times.Never);
    }

    [TestMethod]
    public async Task Login_WhenPasswordIsWrong_ShouldReturnFailure()
    {
        _dbContext.Users.Add(new User
        {
            UserName = "active-user",
            Email = "active@test.com",
            FullName = "Active User",
            Status = "ACTIVE",
            RoleId = 4,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            UserNameOrEmail = "active@test.com",
            Password = "wrong-password"
        };

        var result = await _sut.Login(request);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Email/Username or password is invalid");
    }

    [TestMethod]
    public async Task Login_WhenUserIsInactive_ShouldReturnFailure()
    {
        _dbContext.Users.Add(new User
        {
            UserName = "inactive-user",
            Email = "inactive@test.com",
            FullName = "Inactive User",
            Status = "INACTIVE",
            RoleId = 4,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            UserNameOrEmail = "inactive@test.com",
            Password = "123456"
        };

        var result = await _sut.Login(request);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Your account is not active");
    }

    [TestMethod]
    public async Task VerifyEmail_WhenOtpIsWrong_ShouldReturnFailure()
    {
        var email = "verify-wrong@test.com";
        _dbContext.Users.Add(new User
        {
            UserName = "verify-user",
            Email = email,
            FullName = "Verify User",
            Status = "INACTIVE",
            RoleId = 4,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.Get($"verify:{email}")).Returns("RIGHT1");

        var result = await _sut.VerifyEmail(new VerifyEmailRequest
        {
            Email = email,
            Code = "WRONG1"
        });

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Be("Invalid verification code.");
        _cacheMock.Verify(c => c.Remove(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task VerifyEmail_WhenOtpExpired_ShouldReturnFailure()
    {
        var email = "verify-expired@test.com";
        _cacheMock.Setup(c => c.Get($"verify:{email}")).Returns((string?)null);

        var result = await _sut.VerifyEmail(new VerifyEmailRequest
        {
            Email = email,
            Code = "ABC123"
        });

        result.Success.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.Message.Should().Contain("Code has expired or does not exist");
    }
}
