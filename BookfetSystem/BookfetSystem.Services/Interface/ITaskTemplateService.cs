using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface ITaskTemplateService
    {
        Task<PagedResponse<TaskTemplateResponse>> GetTaskTemplatesAsync(TaskTemplateFilterRequest request, int page, int pageSize);
        Task<ApiResponse<TaskTemplateResponse>> CreateAsync(int ownerId, TaskTemplateCreateRequest request);
        Task<ApiResponse<TaskTemplateResponse>> UpdateAsync(int id, int ownerId, TaskTemplateUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id, int ownerId);
    }
}
