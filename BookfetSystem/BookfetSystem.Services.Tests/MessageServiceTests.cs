using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Hubs;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class MessageServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private MessageService _sut = null!;
    private Mock<IHubContext<ChatHub>> _hubContextMock = null!;
    private Mock<IHubClients> _hubClientsMock = null!;
    private Mock<IClientProxy> _groupClientProxyMock = null!;
    private Mock<IClientProxy> _userClientProxyMock = null!;
    private Mock<INotificationService> _notificationServiceMock = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);

        _groupClientProxyMock = new Mock<IClientProxy>();
        _groupClientProxyMock
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userClientProxyMock = new Mock<IClientProxy>();
        _userClientProxyMock
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _hubClientsMock = new Mock<IHubClients>();
        _hubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_groupClientProxyMock.Object);
        _hubClientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(_userClientProxyMock.Object);

        _hubContextMock = new Mock<IHubContext<ChatHub>>();
        _hubContextMock.SetupGet(x => x.Clients).Returns(_hubClientsMock.Object);

        _notificationServiceMock = new Mock<INotificationService>();
        _notificationServiceMock
            .Setup(x => x.SendToUserAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BookfetSystem.Services.Enum.NotificationType>(),
                It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        _sut = new MessageService(
            new MessageRepository(_dbContext),
            new ConversationRepository(_dbContext),
            new UserRepository(_dbContext),
            new MenuRepository(_dbContext),
            _hubContextMock.Object,
            _notificationServiceMock.Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
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

        _dbContext.Users.AddRange(
            new User
            {
                UserId = 1,
                FullName = "Customer One",
                Email = "c1@test.com",
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
                FullName = "Owner One",
                Email = "o1@test.com",
                Phone = "0900000002",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "owner1",
                Address = "HN",
                RoleId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 3,
                FullName = "Not In Conversation",
                Email = "u3@test.com",
                Phone = "0900000003",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "user3",
                Address = "HN",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.Conversations.Add(new Conversation
        {
            ConversationId = 6001,
            CustomerId = 1,
            OwnerId = 2,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        });

        _dbContext.Messages.AddRange(
            new Message
            {
                MessageId = 60001,
                ConversationId = 6001,
                SenderId = 1,
                Content = "Hello",
                MessageType = "TEXT",
                SentAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Message
            {
                MessageId = 60002,
                ConversationId = 6001,
                SenderId = 2,
                Content = "Hi",
                MessageType = "TEXT",
                SentAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 59 - GetAllMessagesFiltered
    //Function 59 - TC1
    [TestMethod]
    public async Task GetAllMessageFilteredAsync_WhenFilterBySender_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllMessageFilteredAsync(
            new MessageFilterRequest { SenderId = 1 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().MessageId.Should().Be(60001);
        result.Items.First().SenderName.Should().Be("Customer One");
    }

    //Function 59 - TC2
    [TestMethod]
    public async Task GetAllMessageFilteredAsync_WhenPaged_ShouldReturnCorrectPage()
    {
        var result = await _sut.GetAllMessageFilteredAsync(
            new MessageFilterRequest { ConversationId = 6001 },
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().MessageId.Should().Be(60001);
    }
    #endregion

    #region Function 60 - CreateMessage
    //Function 60 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenConversationNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 99999,
            SenderId = 1,
            Content = "Hello",
            MessageType = "TEXT"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Conversation not found.");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenSenderNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 99999,
            Content = "Hello",
            MessageType = "TEXT"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sender not found.");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenSenderNotInConversation_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 3,
            Content = "Hello",
            MessageType = "TEXT"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sender not in conversation.");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenTextWithoutContent_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = "   ",
            MessageType = "TEXT"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Content is required for TEXT message");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenMenuMessageWithoutMenuId_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = null,
            MessageType = "MENU",
            MenuId = null
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("MenuId is required for MENU message");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC6
    [TestMethod]
    public async Task CreateAsync_WhenMenuMessageWithInvalidMenu_ShouldFail()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = null,
            MessageType = "MENU",
            MenuId = 999
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Menu not found");
        result.Data.Should().BeNull();
    }

    //Function 60 - TC7
    [TestMethod]
    public async Task CreateAsync_WhenValidTextMessage_ShouldCreateAndPushRealtime()
    {
        var result = await _sut.CreateAsync(new MessageCreateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = "  New message  ",
            MessageType = "TEXT"
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Message sent");
        result.Data.Should().NotBeNull();
        result.Data!.ConversationId.Should().Be(6001);
        result.Data.SenderId.Should().Be(1);
        result.Data.Content.Should().Be("New message");
        result.Data.SenderName.Should().Be("Customer One");

        _groupClientProxyMock.Verify(x => x.SendCoreAsync(
                "ReceiveMessage",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _userClientProxyMock.Verify(x => x.SendCoreAsync(
                "PushNotification",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }
    #endregion

    #region Function 61 - UpdateMessage
    //Function 61 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenMessageNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new MessageUpdateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = "Updated"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Message not found.");
        result.Data.Should().BeNull();
    }

    //Function 61 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenConversationNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(60001, new MessageUpdateRequest
        {
            ConversationId = 99999,
            SenderId = 1,
            Content = "Updated"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Conversation not found.");
        result.Data.Should().BeNull();
    }

    //Function 61 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenSenderNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(60001, new MessageUpdateRequest
        {
            ConversationId = 6001,
            SenderId = 99999,
            Content = "Updated"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sender not found.");
        result.Data.Should().BeNull();
    }

    //Function 61 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenSenderNotBelongConversation_ShouldFail()
    {
        var result = await _sut.UpdateAsync(60001, new MessageUpdateRequest
        {
            ConversationId = 6001,
            SenderId = 3,
            Content = "Updated"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Sender must be the customer or owner of this conversation.");
        result.Data.Should().BeNull();
    }

    //Function 61 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(60001, new MessageUpdateRequest
        {
            ConversationId = 6001,
            SenderId = 1,
            Content = "  Customer edited  "
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Message updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Content.Should().Be("Customer edited");

        var saved = await _dbContext.Messages.AsNoTracking().FirstAsync(x => x.MessageId == 60001);
        saved.SenderId.Should().Be(1);
        saved.Content.Should().Be("Customer edited");
    }
    #endregion
}

