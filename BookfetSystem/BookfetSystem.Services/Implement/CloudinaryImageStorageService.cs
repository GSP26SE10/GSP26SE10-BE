using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Options;
using BookfetSystem.Services.Enum;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement;

public class CloudinaryImageStorageService : IImageStorageService
{
    private static readonly string[] AllowedImageContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryImageStorageService(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.CloudName) ||
            string.IsNullOrWhiteSpace(_options.ApiKey) ||
            string.IsNullOrWhiteSpace(_options.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing (Cloudinary:CloudName/ApiKey/ApiSecret).");
        }

        var account = new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public Task<string> UploadMenuImageAsync(IFormFile file, int? menuId = null, CancellationToken ct = default)
    {
        return UploadImageAsync(file, CloudinaryFolder.Menu, menuId, ct);
    }

    public async Task<string> UploadImageAsync(IFormFile file, CloudinaryFolder folder, int? entityId = null, CancellationToken ct = default)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (file.Length <= 0) throw new InvalidOperationException("Image file is empty.");

        var contentType = file.ContentType?.Trim();
        var isAllowed = Array.Exists(AllowedImageContentTypes, x => string.Equals(x, contentType, StringComparison.OrdinalIgnoreCase));
        if (!isAllowed)
        {
            throw new InvalidOperationException("Only JPG/PNG/WEBP images are allowed.");
        }

        await using var stream = file.OpenReadStream();

        var folderName = ResolveFolderName(folder);
        var entityPrefix = folderName;
        var publicId = entityId.HasValue
            ? $"{entityPrefix}_{entityId.Value}_{Guid.NewGuid():N}"
            : $"{entityPrefix}_{Guid.NewGuid():N}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folderName,
            AssetFolder = folderName,
            PublicId = publicId,
            Overwrite = false,
            UseFilename = false,
            UniqueFilename = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);
        if (result.StatusCode is not (System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Created) || result.SecureUrl == null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error?.Message ?? "Unknown error"}");
        }

        return result.SecureUrl.ToString();
    }

    private static string ResolveFolderName(CloudinaryFolder folder)
    {
        return folder switch
        {
            CloudinaryFolder.Menu => "menu",
            CloudinaryFolder.Dish => "dish",
            CloudinaryFolder.Service => "service",
            CloudinaryFolder.FeedbackMenu => "feedbackMenu",
            CloudinaryFolder.FeedbackService => "feedbackService",
            CloudinaryFolder.ExtraCharge => "extraCharge",
            _ => throw new InvalidOperationException("Unsupported cloudinary folder.")
        };
    }
}

