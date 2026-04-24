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
public class PostServiceTests
{
    private GSP26SE10DBContext _dbContext = null!;
    private PostService _sut = null!;
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
            .ReturnsAsync("https://cdn.test/post/default.jpg");

        _sut = new PostService(
            new PostRepository(_dbContext),
            new BlogCategoryRepository(_dbContext),
            _imageStorageServiceMock.Object);

        await SeedBaseDataAsync();
    }

    private async Task SeedBaseDataAsync()
    {
        _dbContext.BlogCategories.AddRange(
            new BlogCategory { BlogCategoryId = 1, Name = "Tips", Slug = "tips" },
            new BlogCategory { BlogCategoryId = 2, Name = "News", Slug = "news" });

        _dbContext.Posts.AddRange(
            new Post
            {
                PostId = 6701,
                BlogCategoryId = 1,
                Slug = "intro-post",
                Title = "Intro Post",
                Excerpt = "hello",
                Coverimage = null,
                Status = PostStatus.Published.ToString(),
                PublishedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Post
            {
                PostId = 6702,
                BlogCategoryId = 2,
                Slug = "draft-post",
                Title = "Draft Post",
                Excerpt = "draft",
                Coverimage = null,
                Status = PostStatus.Draft.ToString(),
                PublishedAt = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            });

        await _dbContext.SaveChangesAsync();
    }

    private static IFormFile CreateImage(string name)
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        return new FormFile(stream, 0, stream.Length, "CoverImageFiles", name);
    }

    #region Function 67 - GetAllPostsFiltered
    //Function 67 - TC1
    [TestMethod]
    public async Task GetAllPostFilteredAsync_WhenFilterByStatus_ShouldReturnMatchedRows()
    {
        var result = await _sut.GetAllPostFilteredAsync(
            new PostFilterRequest { Status = PostStatus.Published },
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().PostId.Should().Be(6701);
        result.Items.First().Status.Should().Be(PostStatus.Published.ToString());
    }

    //Function 67 - TC2
    [TestMethod]
    public async Task GetAllPostFilteredAsync_WhenPaged_ShouldReturnExpectedPage()
    {
        var result = await _sut.GetAllPostFilteredAsync(
            new PostFilterRequest(),
            page: 2,
            pageSize: 1);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.Items.First().PostId.Should().Be(6701);
    }
    #endregion

    #region Function 68 - CreatePost
    //Function 68 - TC1
    [TestMethod]
    public async Task CreateAsync_WhenSlugMissing_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = " ",
            Title = "Any",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Slug is required.");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC2
    [TestMethod]
    public async Task CreateAsync_WhenTitleMissing_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "new-post",
            Title = " ",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Title is required.");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC3
    [TestMethod]
    public async Task CreateAsync_WhenBlogCategoryNotFound_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "new-post",
            Title = "New Post",
            Status = PostStatus.Draft,
            BlogCategoryId = 999
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("BlogCategoryId does not exist.");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC4
    [TestMethod]
    public async Task CreateAsync_WhenSlugDuplicated_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "INTRO-POST",
            Title = "New Post",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Slug already exists.");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC5
    [TestMethod]
    public async Task CreateAsync_WhenCoverImageTooMany_ShouldFail()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "new-post",
            Title = "New Post",
            Status = PostStatus.Draft,
            BlogCategoryId = 1,
            CoverImageFiles = new List<IFormFile>
            {
                CreateImage("1.jpg"), CreateImage("2.jpg"), CreateImage("3.jpg"),
                CreateImage("4.jpg"), CreateImage("5.jpg"), CreateImage("6.jpg")
            }
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Maximum 5 cover images are allowed per request.");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC6
    [TestMethod]
    public async Task CreateAsync_WhenUploadImageFails_ShouldFail()
    {
        _imageStorageServiceMock.Setup(x => x.UploadImageAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CloudinaryFolder>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("upload error"));

        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "new-post",
            Title = "New Post",
            Status = PostStatus.Draft,
            BlogCategoryId = 1,
            CoverImageFiles = new List<IFormFile> { CreateImage("1.jpg") }
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to upload post cover images: upload error");
        result.Data.Should().BeNull();
    }

    //Function 68 - TC7
    [TestMethod]
    public async Task CreateAsync_WhenValidPublished_ShouldCreateWithPublishedAt()
    {
        var result = await _sut.CreateAsync(new PostCreateRequest
        {
            Slug = "  fresh-post  ",
            Title = "  Fresh Post  ",
            Excerpt = "  excerpt  ",
            Status = PostStatus.Published,
            BlogCategoryId = 1
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Post created successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Slug.Should().Be("fresh-post");
        result.Data.Title.Should().Be("Fresh Post");
        result.Data.Status.Should().Be(PostStatus.Published.ToString());
        result.Data.PublishedAt.Should().NotBeNull();
    }
    #endregion

    #region Function 69 - UpdatePost
    //Function 69 - TC1
    [TestMethod]
    public async Task UpdateAsync_WhenPostNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(99999, new PostUpdateRequest
        {
            Slug = "any",
            Title = "any",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Post not found.");
        result.Data.Should().BeNull();
    }

    //Function 69 - TC2
    [TestMethod]
    public async Task UpdateAsync_WhenSlugMissing_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6701, new PostUpdateRequest
        {
            Slug = " ",
            Title = "Updated",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Slug is required.");
        result.Data.Should().BeNull();
    }

    //Function 69 - TC3
    [TestMethod]
    public async Task UpdateAsync_WhenTitleMissing_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6701, new PostUpdateRequest
        {
            Slug = "updated",
            Title = " ",
            Status = PostStatus.Draft,
            BlogCategoryId = 1
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Title is required.");
        result.Data.Should().BeNull();
    }

    //Function 69 - TC4
    [TestMethod]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6701, new PostUpdateRequest
        {
            Slug = "updated",
            Title = "Updated",
            Status = PostStatus.Draft,
            BlogCategoryId = 999
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("BlogCategoryId does not exist.");
        result.Data.Should().BeNull();
    }

    //Function 69 - TC5
    [TestMethod]
    public async Task UpdateAsync_WhenSlugDuplicated_ShouldFail()
    {
        var result = await _sut.UpdateAsync(6702, new PostUpdateRequest
        {
            Slug = "INTRO-POST",
            Title = "Updated",
            Status = PostStatus.Draft,
            BlogCategoryId = 2
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Slug already exists.");
        result.Data.Should().BeNull();
    }

    //Function 69 - TC6
    [TestMethod]
    public async Task UpdateAsync_WhenValid_ShouldUpdateSuccessfully()
    {
        var result = await _sut.UpdateAsync(6702, new PostUpdateRequest
        {
            Slug = "  draft-post  ",
            Title = "  Draft Post Updated  ",
            Excerpt = "  new excerpt ",
            Status = PostStatus.Draft,
            BlogCategoryId = 2
        });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Post updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Draft Post Updated");

        var saved = await _dbContext.Posts.AsNoTracking().FirstAsync(x => x.PostId == 6702);
        saved.Title.Should().Be("Draft Post Updated");
        saved.Excerpt.Should().Be("new excerpt");
    }
    #endregion

    #region Function 70 - DeletePost
    //Function 70 - TC1
    [TestMethod]
    public async Task DeleteAsync_WhenPostNotFound_ShouldFail()
    {
        var result = await _sut.DeleteAsync(99999);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Post not found.");
        result.Data.Should().BeFalse();
    }

    //Function 70 - TC2
    [TestMethod]
    public async Task DeleteAsync_WhenValid_ShouldDeleteSuccessfully()
    {
        var result = await _sut.DeleteAsync(6702);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Post deleted successfully.");
        result.Data.Should().BeTrue();

        (await _dbContext.Posts.AnyAsync(x => x.PostId == 6702)).Should().BeFalse();
    }
    #endregion
}

