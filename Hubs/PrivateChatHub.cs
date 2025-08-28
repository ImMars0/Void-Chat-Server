using Microsoft.AspNetCore.SignalR;
using Void.Database;
using Void.Models;

namespace Void.Hubs
{
    public class PrivateChatHub : Hub
    {
        private readonly DatabaseContext _context;
        private static readonly HashSet<int> ConnectedUsers = new();

        public PrivateChatHub(DatabaseContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserIdFromContext();
            if (userId.HasValue) ConnectedUsers.Add(userId.Value);
            await Clients.All.SendAsync("OnlineUsersCount", ConnectedUsers.Count);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserIdFromContext();
            if (userId.HasValue) ConnectedUsers.Remove(userId.Value);
            await Clients.All.SendAsync("OnlineUsersCount", ConnectedUsers.Count);
            await base.OnDisconnectedAsync(exception);
        }

        private int? GetUserIdFromContext()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userIdStr))
                if (int.TryParse(userIdStr, out var userId)) return userId;
            return null;
        }

        public async Task SendMessageToUser(int receiverId, string message)
        {
            var senderId = GetUserIdFromContext();
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

            await Clients.User(receiverId.ToString()).SendAsync("ReceivePrivateMessage", chat);
            await Clients.Caller.SendAsync("ReceivePrivateMessage", chat);
        }
    }
}


