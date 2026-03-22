using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BookfetSystem.Services.Hubs;
using System;
using System.Threading.Tasks;
using System.Reflection;

namespace BookfetSystem.Services.Implement
{
    public class MessageService : IMessageService
    {
        private readonly MessageRepository _messageRepository;
        private readonly ConversationRepository _conversationRepository;
        private readonly UserRepository _userRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly INotificationService _notificationService;

        public MessageService(
            MessageRepository messageRepository,
            ConversationRepository conversationRepository,
            UserRepository userRepository,
            IHubContext<ChatHub> hubContext,
            INotificationService notificationService)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task<PagedResponse<MessageResponse>> GetAllMessageFilteredAsync(MessageFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Message>();
            var query = _messageRepository.GetAllMessageFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<MessageResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<MessageResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<MessageResponse>> CreateAsync(MessageCreateRequest request)
        {
            var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
            var sender = await _userRepository.GetByIdAsync(request.SenderId);
            if (conversation == null)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Conversation not found.",
                    Data = null
                };
            }

            if (sender == null)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Sender not found.",
                    Data = null
                };
            }

            if (request.SenderId != conversation.CustomerId && request.SenderId != conversation.OwnerId)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Sender not in conversation.",
                    Data = null
                };
            }

            var entity = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content?.Trim(),
                SentAt = DateTime.UtcNow
            };

            var affected = await _messageRepository.CreateAsync(entity);

            if (affected > 0)
            {
                var response = new MessageResponse
                {
                    MessageId = entity.MessageId,
                    ConversationId = entity.ConversationId,
                    SenderId = entity.SenderId,
                    Content = entity.Content,
                    SentAt = entity.SentAt,
                    SenderName = sender.FullName
                };

                // ?? REALTIME (ROOM)
                await _hubContext.Clients
                    .Group(entity.ConversationId.ToString())
                    .SendAsync("ReceiveMessage", response);

                var receiverId = request.SenderId == conversation.CustomerId
                    ? conversation.OwnerId
                    : conversation.CustomerId;

                if (receiverId.HasValue)
                {
                    await _hubContext.Clients
                        .User(receiverId.Value.ToString())
                        .SendAsync("PushNotification", new
                        {
                            Title = "Bạn có tin nhắn mới",
                            Body = response.Content,
                            ConversationId = response.ConversationId
                        });

                    var notificationBody = string.IsNullOrWhiteSpace(response.Content)
                        ? $"{sender.FullName} đã gửi cho bạn một tin nhắn mới."
                        : $"{sender.FullName}: {response.Content}";

                    await _notificationService.SendToUserAsync(
                        receiverId.Value,
                        "Bạn có tin nhắn mới",
                        notificationBody,
                        NotificationType.Message,
                        new Dictionary<string, string>
                        {
                            ["conversationId"] = response.ConversationId.ToString(),
                            ["messageId"] = response.MessageId.ToString(),
                            ["senderId"] = response.SenderId?.ToString() ?? string.Empty
                        });
                }

                return new ApiResponse<MessageResponse>
                {
                    Success = true,
                    Message = "Message sent",
                    Data = response
                };
            }

            return new ApiResponse<MessageResponse>
            {
                Success = false,
                Message = "Failed to create message",
                Data = null
            };
        }

        public async Task<ApiResponse<MessageResponse>> UpdateAsync(int id, MessageUpdateRequest request)
        {
            var entity = await _messageRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Message not found.",
                    Data = null
                };
            }

            var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Conversation not found.",
                    Data = null
                };
            }

            var sender = await _userRepository.GetByIdAsync(request.SenderId);
            if (sender == null)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Sender not found.",
                    Data = null
                };
            }

            if (request.SenderId != conversation.CustomerId && request.SenderId != conversation.OwnerId)
            {
                return new ApiResponse<MessageResponse>
                {
                    Success = false,
                    Message = "Sender must be the customer or owner of this conversation.",
                    Data = null
                };
            }

            entity.ConversationId = request.ConversationId;
            entity.SenderId = request.SenderId;
            entity.Content = request.Content?.Trim();

            var affected = await _messageRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = new MessageResponse
                {
                    MessageId = entity.MessageId,
                    ConversationId = entity.ConversationId,
                    SenderId = entity.SenderId,
                    Content = entity.Content,
                    SentAt = entity.SentAt,
                    SenderName = sender.FullName
                };

                return new ApiResponse<MessageResponse>
                {
                    Success = true,
                    Message = "Message updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MessageResponse>
            {
                Success = false,
                Message = "Failed to update message.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _messageRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Message not found.",
                    Data = false
                };
            }

            var removed = await _messageRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Message deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete message.",
                Data = false
            };
        }
    }
}
