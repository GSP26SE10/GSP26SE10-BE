using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Hubs
{
    public class ChatHub : Hub
    {
        // Join vào 1 conversation (room)
        public async Task JoinConversation(JsonElement conversationId)
        {
            var groupName = ParseConversationGroupName(conversationId);
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new HubException("conversationId is required.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave room
        public async Task LeaveConversation(JsonElement conversationId)
        {
            var groupName = ParseConversationGroupName(conversationId);
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new HubException("conversationId is required.");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // (Optional) tracking online
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private static string ParseConversationGroupName(JsonElement rawConversationId)
        {
            return rawConversationId.ValueKind switch
            {
                JsonValueKind.String => rawConversationId.GetString()?.Trim() ?? string.Empty,
                JsonValueKind.Number => rawConversationId.ToString(),
                JsonValueKind.Object => TryReadFromObject(rawConversationId),
                _ => string.Empty
            };
        }

        private static string TryReadFromObject(JsonElement rawConversationId)
        {
            if (rawConversationId.TryGetProperty("conversationId", out var idValue))
            {
                if (idValue.ValueKind == JsonValueKind.Number)
                {
                    return idValue.ToString();
                }

                if (idValue.ValueKind == JsonValueKind.String)
                {
                    return idValue.GetString()?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
