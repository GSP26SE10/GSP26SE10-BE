using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface ITaskTemplateService
    {
        Task<PagedResponse<TaskTemplateResponse>> GetTaskTemplatesAsync(TaskTemplateFilterRequest request, int page, int pageSize);
        Task<ApiResponse<TaskTemplateResponse>> GetByIdAsync(int id);
        Task<ApiResponse<TaskTemplateResponse>> CreateAsync(TaskTemplateCreateRequest request);
        Task<ApiResponse<TaskTemplateResponse>> UpdateAsync(int id, TaskTemplateUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
