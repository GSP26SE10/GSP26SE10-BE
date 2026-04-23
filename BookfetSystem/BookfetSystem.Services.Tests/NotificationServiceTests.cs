using System.Net;
using System.Net.Http;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Options;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class NotificationServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private NotificationService _sut = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private Func<HttpResponseMessage> _responseFactory = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _responseFactory = () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"data\":[{\"status\":\"ok\"}]}")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new DelegateHttpMessageHandler(_ => _responseFactory())));

        var expoProvider = new ExpoPushProvider(
            _httpClientFactoryMock.Object,
            new Mock<ILogger<ExpoPushProvider>>().Object);

        var fcmProvider = new FcmPushProvider(
            Microsoft.Extensions.Options.Options.Create(new FirebaseOptions { ProjectId = "test-project", CredentialsPath = string.Empty, Enabled = true }),
            new Mock<ILogger<FcmPushProvider>>().Object);

        _sut = new NotificationService(
            new NotificationRepository(_dbContext),
            new UserDeviceRepository(_dbContext),
            expoProvider,
            fcmProvider,
            new Mock<ILogger<NotificationService>>().Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.Users.AddRange(
            new User
            {
                UserId = 1,
                FullName = "User One",
                Email = "u1@test.com",
                Phone = "0900000001",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "u1",
                Address = "HN",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                FullName = "User Two",
                Email = "u2@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "u2",
                Address = "HN",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 40 - Get All Notifications Filtered
    //Function 40 - TC1
    [TestMethod]
    public async Task GetAllNotificationFilteredAsync_WhenFilterByType_ShouldReturnMatchedRows()
    {
        _dbContext.Notifications.AddRange(
            new Notification
            {
                NotificationId = 4001,
                UserId = 1,
                Title = "Order 1",
                Content = "Order content",
                Type = ((int)NotificationType.Order).ToString(),
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Notification
            {
                NotificationId = 4002,
                UserId = 1,
                Title = "Task 1",
                Content = "Task content",
                Type = ((int)NotificationType.Task).ToString(),
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new Notification
            {
                NotificationId = 4003,
                UserId = 2,
                Title = "Order 2",
                Content = "Other user content",
                Type = ((int)NotificationType.Order).ToString(),
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow
            });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetAllNotificationFilteredAsync(
            new NotificationFilterRequest { Type = NotificationType.Order },
            userId: 1,
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().NotificationId.Should().Be(4001);
        result.Items.First().Type.Should().Be((int)NotificationType.Order);
        result.Items.First().Body.Should().Be("Order content");
    }

    //Function 40 - TC2
    [TestMethod]
    public async Task GetAllNotificationFilteredAsync_WhenPaged_ShouldReturnCorrectPage()
    {
        _dbContext.Notifications.AddRange(
            new Notification
            {
                NotificationId = 4011,
                UserId = 1,
                Title = "N1",
                Content = "C1",
                Type = "1",
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new Notification
            {
                NotificationId = 4012,
                UserId = 1,
                Title = "N2",
                Content = "C2",
                Type = "1",
                IsRead = true,
                IsSent = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Notification
            {
                NotificationId = 4013,
                UserId = 1,
                Title = "N3",
                Content = "C3",
                Type = "4",
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GetAllNotificationFilteredAsync(
            new NotificationFilterRequest(),
            userId: 1,
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().NotificationId.Should().Be(4012);
    }
    #endregion

    #region Function 41 - Create Notification
    //Function 41 - TC1
    [TestMethod]
    public async Task SendToUserAsync_WhenNoDevice_ShouldStillSaveInAppNotification()
    {
        await _sut.SendToUserAsync(1, "Hello", "Body text", NotificationType.System);

        var saved = await _dbContext.Notifications.AsNoTracking().SingleAsync(x => x.UserId == 1);
        saved.Title.Should().Be("Hello");
        saved.Content.Should().Be("Body text");
        saved.Type.Should().Be(((int)NotificationType.System).ToString());
        saved.IsRead.Should().BeFalse();
        saved.IsSent.Should().BeTrue();
    }

    //Function 41 - TC2
    [TestMethod]
    public async Task SendToUserAsync_WhenExpoPushFails_ShouldDeactivateToken()
    {
        _responseFactory = () => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":\"DeviceNotRegistered\"}")
        };

        _dbContext.UserDevices.Add(new UserDevice
        {
            UserDeviceId = 5001,
            UserId = 1,
            DeviceId = "d1",
            ExpoPushToken = "ExponentPushToken[test-token]",
            Platform = "ios",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        await _sut.SendToUserAsync(1, "Push", "Fail body", NotificationType.Order);

        var device = await _dbContext.UserDevices.AsNoTracking().FirstAsync(x => x.UserDeviceId == 5001);
        device.IsActive.Should().BeFalse();

        (await _dbContext.Notifications.CountAsync(x => x.UserId == 1)).Should().Be(1);
    }
    #endregion

    #region Function 42 - Update Notification
    //Function 42 - TC1
    [TestMethod]
    public async Task MarkAsReadAsync_WhenNotificationNotFound_ShouldFail()
    {
        var result = await _sut.MarkAsReadAsync(notificationId: 99999, userId: 1);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Notification not found.");
        result.Data.Should().BeNull();
    }

    //Function 42 - TC2
    [TestMethod]
    public async Task MarkAsReadAsync_WhenBelongsToAnotherUser_ShouldFail()
    {
        _dbContext.Notifications.Add(new Notification
        {
            NotificationId = 4201,
            UserId = 2,
            Title = "Private",
            Content = "Content",
            Type = "1",
            IsRead = false,
            IsSent = true,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.MarkAsReadAsync(notificationId: 4201, userId: 1);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Notification not found.");
        result.Data.Should().BeNull();
    }

    //Function 42 - TC3
    [TestMethod]
    public async Task MarkAsReadAsync_WhenValid_ShouldUpdateIsRead()
    {
        _dbContext.Notifications.Add(new Notification
        {
            NotificationId = 4202,
            UserId = 1,
            Title = "Unread",
            Content = "Body",
            Type = NotificationType.Task.ToString(),
            IsRead = false,
            IsSent = true,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.MarkAsReadAsync(notificationId: 4202, userId: 1);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Marked as read successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.IsRead.Should().BeTrue();
        result.Data.Type.Should().Be((int)NotificationType.Task);

        var saved = await _dbContext.Notifications.AsNoTracking().FirstAsync(x => x.NotificationId == 4202);
        saved.IsRead.Should().BeTrue();
    }
    #endregion

    private sealed class DelegateHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public DelegateHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}

