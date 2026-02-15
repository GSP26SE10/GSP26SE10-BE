using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUserFilteredAsync(UserFilterRequest request, int page, int pageSize);
        Task<ApiResponse<UserResponse>> CreateAsync(UserCreateRequest request);
        Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
