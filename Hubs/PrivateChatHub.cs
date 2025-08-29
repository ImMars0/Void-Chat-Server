using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Claims;
using Void.Database;
using Void.Models;

namespace Void.Hubs
{
    public class PrivateChatHub : Hub
    {
        private readonly DatabaseContext _context;

        private static readonly ConcurrentDictionary<int, HashSet<string>> ConnectedUsers = new();

        public PrivateChatHub(DatabaseContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdFromClaims();
            if (userId != null)
            {
                ConnectedUsers.AddOrUpdate(
                    userId.Value,
                    _ => new HashSet<string> { Context.ConnectionId },
                    (_, set) => { set.Add(Context.ConnectionId); return set; });

                Debug.WriteLine($"Client connected: {Context.ConnectionId} (UserId: {userId})");
                await Clients.All.SendAsync("OnlineUsersCount", ConnectedUsers.Count);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserIdFromClaims();
            if (userId != null && ConnectedUsers.TryGetValue(userId.Value, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                    ConnectedUsers.TryRemove(userId.Value, out _);

                Debug.WriteLine($"Client disconnected: {Context.ConnectionId} (UserId: {userId})");
                await Clients.All.SendAsync("OnlineUsersCount", ConnectedUsers.Count);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }
        public Task<bool> IsUserOnline(int userId)
        {
            return Task.FromResult(ConnectedUsers.ContainsKey(userId));
        }


        public async Task SendPrivateMessage(int receiverId, string message)
        {
            var senderId = GetUserIdFromClaims();
            if (!senderId.HasValue) return;

            var chat = new Chat
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
            {
                chat.Id,
                chat.SenderId,
                chat.ReceiverId,
                chat.Content,
                chat.Timestamp,
                chat.IsRead
            });
        }
    }
}
