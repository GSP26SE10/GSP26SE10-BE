using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
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
    public class ConversationService : IConversationService
    {
        private readonly ConversationRepository _conversationRepository;
        private readonly UserRepository _userRepository;

        public ConversationService(ConversationRepository conversationRepository, UserRepository userRepository)
        {
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<ConversationResponse>> GetAllConversationFilteredAsync(ConversationFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Conversation>();
            var query = _conversationRepository.GetAllConversationFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<ConversationResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<ConversationResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ConversationResponse>> CreateAsync(ConversationCreateRequest request)
        {
            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<ConversationResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var owner = await _userRepository.GetByIdAsync(request.OwnerId);
            if (owner == null)
            {
                return new ApiResponse<ConversationResponse>
                {
                    Success = false,
                    Message = "Owner not found.",
                    Data = null
                };
            }

            var entity = new Conversation
            {
                CustomerId = request.CustomerId,
                OwnerId = request.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _conversationRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = new ConversationResponse
                {
                    ConversationId = entity.ConversationId,
                    CustomerId = entity.CustomerId,
                    OwnerId = entity.OwnerId,
                    CreatedAt = entity.CreatedAt,
                    CustomerName = customer.FullName,
                    OwnerName = owner.FullName
                };

                return new ApiResponse<ConversationResponse>
                {
                    Success = true,
                    Message = "Conversation created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<ConversationResponse>
            {
                Success = false,
                Message = "Failed to create conversation.",
                Data = null
            };
        }

        public async Task<ApiResponse<ConversationResponse>> UpdateAsync(int id, ConversationUpdateRequest request)
        {
            var entity = await _conversationRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<ConversationResponse>
                {
                    Success = false,
                    Message = "Conversation not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<ConversationResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var owner = await _userRepository.GetByIdAsync(request.OwnerId);
            if (owner == null)
            {
                return new ApiResponse<ConversationResponse>
                {
                    Success = false,
                    Message = "Owner not found.",
                    Data = null
                };
            }

            entity.CustomerId = request.CustomerId;
            entity.OwnerId = request.OwnerId;

            var affected = await _conversationRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = new ConversationResponse
                {
                    ConversationId = entity.ConversationId,
                    CustomerId = entity.CustomerId,
                    OwnerId = entity.OwnerId,
                    CreatedAt = entity.CreatedAt,
                    CustomerName = customer.FullName,
                    OwnerName = owner.FullName
                };

                return new ApiResponse<ConversationResponse>
                {
                    Success = true,
                    Message = "Conversation updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<ConversationResponse>
            {
                Success = false,
                Message = "Failed to update conversation.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _conversationRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Conversation not found.",
                    Data = false
                };
            }

            var removed = await _conversationRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Conversation deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete conversation.",
                Data = false
            };
        }
    }
}
