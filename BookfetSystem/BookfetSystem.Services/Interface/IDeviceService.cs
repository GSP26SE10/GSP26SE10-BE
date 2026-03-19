using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;

namespace BookfetSystem.Services.Interface
{
    public interface IDeviceService
    {
        Task<ApiResponse<bool>> RegisterAsync(DeviceRegisterRequest request);

        Task<ApiResponse<bool>> DeactivateAsync(DeviceDeactivateRequest request);
    }
}