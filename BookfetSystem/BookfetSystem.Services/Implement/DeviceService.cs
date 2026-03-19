using BookfetSystem.Repositories;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;

namespace BookfetSystem.Services.Implement
{
    public class DeviceService : IDeviceService
    {
        private readonly UserRepository _userRepository;
        private readonly UserDeviceRepository _userDeviceRepository;

        public DeviceService(UserRepository userRepository, UserDeviceRepository userDeviceRepository)
        {
            _userRepository = userRepository;
            _userDeviceRepository = userDeviceRepository;
        }

        public async Task<ApiResponse<bool>> RegisterAsync(DeviceRegisterRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false
                };
            }

            var platform = request.Platform.Trim().ToLowerInvariant();
            if (platform != "android" && platform != "ios")
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Platform must be 'android' or 'ios'.",
                    Data = false
                };
            }

            await _userDeviceRepository.UpsertByDeviceIdAsync(
                request.UserId,
                request.DeviceId.Trim(),
                request.ExpoPushToken.Trim(),
                platform,
                request.IsActive);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Device registered successfully.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(DeviceDeactivateRequest request)
        {
            var affected = await _userDeviceRepository.DeactivateByDeviceIdAsync(request.DeviceId.Trim());

            return new ApiResponse<bool>
            {
                Success = true,
                Message = affected > 0 ? "Device deactivated successfully." : "No active device found with the given DeviceId.",
                Data = true
            };
        }
    }
}