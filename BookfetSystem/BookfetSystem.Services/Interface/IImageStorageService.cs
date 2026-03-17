using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Interface;

public interface IImageStorageService
{
    Task<string> UploadImageAsync(IFormFile file, CloudinaryFolder folder, int? entityId = null, CancellationToken ct = default);
    Task<string> UploadMenuImageAsync(IFormFile file, int? menuId = null, CancellationToken ct = default);
}

