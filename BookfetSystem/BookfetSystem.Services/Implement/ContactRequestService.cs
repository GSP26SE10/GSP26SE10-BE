using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class ContactRequestService : IContactRequestService
    {
        private readonly ContactRequestRepository _repository;

        public ContactRequestService(ContactRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<ContactRequestResponse>> GetAllFilteredAsync(ContactRequestFilterRequest request, int page, int pageSize)
        {
            var filter = request.Adapt<ContactRequest>();
            var query = _repository.GetAllFiltered(filter);

            var total = await query.CountAsync();

            var items = await query
                .ProjectToType<ContactRequestResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<ContactRequestResponse>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ContactRequestResponse>> CreateAsync(ContactRequestCreateRequest request)
        {
            var entity = request.Adapt<ContactRequest>();

            entity.Status = ContactRequestStatus.PENDING.ToString();
            entity.CreatedAt = DateTime.UtcNow;

            var result = await _repository.CreateAsync(entity);

            if (result > 0)
            {
                return new ApiResponse<ContactRequestResponse>
                {
                    Success = true,
                    Message = "Created successfully",
                    Data = entity.Adapt<ContactRequestResponse>()
                };
            }

            return new ApiResponse<ContactRequestResponse>
            {
                Success = false,
                Message = "Create failed"
            };
        }

        public async Task<ApiResponse<ContactRequestResponse>> UpdateAsync(int id, ContactRequestUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<ContactRequestResponse>
                {
                    Success = false,
                    Message = "Not found"
                };
            }

            entity.Status = request.Status.ToString();
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(entity);

            if (result > 0)
            {
                return new ApiResponse<ContactRequestResponse>
                {
                    Success = true,
                    Message = "Updated successfully",
                    Data = entity.Adapt<ContactRequestResponse>()
                };
            }

            return new ApiResponse<ContactRequestResponse>
            {
                Success = false,
                Message = "Update failed"
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Not found",
                    Data = false
                };
            }

            var removed = await _repository.RemoveAsync(entity);

            return new ApiResponse<bool>
            {
                Success = removed,
                Message = removed ? "Deleted successfully" : "Delete failed",
                Data = removed
            };
        }
    }
}