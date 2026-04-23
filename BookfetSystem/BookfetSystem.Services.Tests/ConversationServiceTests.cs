using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Models.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookfetSystem.Services.Tests;

[TestClass]
public class ConversationServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private ConversationService _sut = null!;

    [TestInitialize]
    public async Task SetupAsync()
    {
        MapsterTestBootstrap.EnsureConfigured();

        var options = new DbContextOptionsBuilder<GSP26SE10DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new GSP26SE10DBContext(options);
        _sut = new ConversationService(
            new ConversationRepository(_dbContext),
            new UserRepository(_dbContext));

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
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
                FullName = "Customer Two",
                Email = "c2@test.com",
                Phone = "0900000003",
                Avatar = string.Empty,
                Status = "ACTIVE",
                PasswordHash = "hash",
                UserName = "customer2",
                Address = "HCM",
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            });

        _dbContext.Conversations.AddRange(
            new Conversation
            {
                ConversationId = 5601,
                CustomerId = 1,
                OwnerId = 2,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            },
            new Conversation
            {
                ConversationId = 5602,
                CustomerId = 3,
                OwnerId = 2,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });

        await _dbContext.SaveChangesAsync();
    }

    #region Function 56 - GetAllConversationsFiltered
    //Function 56 - TC1
    [TestMethod]
    public async Task GetAllConversationFilteredAsync_WhenFilterByCustomer_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllConversationFilteredAsync(
            new ConversationFilterRequest { CustomerId = 1 },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().ConversationId.Should().Be(5601);
        result.Items.First().CustomerName.Should().Be("Customer One");
        result.Items.First().OwnerName.Should().Be("Owner One");
    }

    //Function 56 - TC2
    [TestMethod]
    public async Task GetAllConversationFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllConversationFilteredAsync(
            new ConversationFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().ConversationId.Should().Be(5601);
    }
    #endregion

    #region Function 57 - CreateConversation
    //Function 57 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenCustomerNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new ConversationCreateRequest { CustomerId = 999, OwnerId = 2 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Customer not found.");
        result.Data.Should().BeNull();
    }

    //Function 57 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenOwnerNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new ConversationCreateRequest { CustomerId = 1, OwnerId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Owner not found.");
        result.Data.Should().BeNull();
    }

    //Function 57 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenValid_ShouldCreateSuccessfully()
    {
        var result = await _sut.CreateAsync(new ConversationCreateRequest { CustomerId = 1, OwnerId = 2 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Conversation created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.CustomerName.Should().Be("Customer One");
        result.Data.OwnerName.Should().Be("Owner One");
        result.Data.ConversationId.Should().BeGreaterThan(0);
    }
    #endregion

    #region Function 58 - UpdateConversation
    //Function 58 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenConversationNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new ConversationUpdateRequest { CustomerId = 1, OwnerId = 2 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Conversation not found.");
        result.Data.Should().BeNull();
    }

    //Function 58 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenCustomerNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(5601, new ConversationUpdateRequest { CustomerId = 999, OwnerId = 2 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Customer not found.");
        result.Data.Should().BeNull();
    }

    //Function 58 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenOwnerNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(5601, new ConversationUpdateRequest { CustomerId = 1, OwnerId = 999 });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Owner not found.");
        result.Data.Should().BeNull();
    }

    //Function 58 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(5601, new ConversationUpdateRequest { CustomerId = 1, OwnerId = 2 });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Conversation updated successfully.");
        result.Data.Should().NotBeNull();

        var saved = await _dbContext.Conversations.AsNoTracking().FirstAsync(x => x.ConversationId == 5601);
        saved.CustomerId.Should().Be(1);
        saved.OwnerId.Should().Be(2);
    }
    #endregion

    #region Function 59 - DeleteConversation
    //Function 59 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenConversationNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Conversation not found.");
        result.Data.Should().BeFalse();
    }

    //Function 59 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(5602);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Conversation deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.Conversations.AnyAsync(x => x.ConversationId == 5602)).Should().BeFalse();
    }
    #endregion
}

