using Microsoft.AspNetCore.SignalR;
using Void.Database;
using Void.Models;

namespace Void.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DatabaseContext _context;
        private static readonly HashSet<int> ConnectedUsers = new();

        public ChatHub(DatabaseContext context)
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
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", chat);
            await Clients.Caller.SendAsync("ReceiveMessage", chat);
        }

        public async Task JoinGroup(int groupId) => await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        public async Task LeaveGroup(int groupId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());

        public async Task SendMessageToGroup(int groupId, string message)
        {
            var senderId = GetUserIdFromContext();
            if (!senderId.HasValue) return;

            var groupMessage = new GroupMessage
            {
                GroupId = groupId,
                SenderId = senderId.Value,
                Content = message,
                Timestamp = DateTime.UtcNow
            };

            _context.GroupMessages.Add(groupMessage);
            await _context.SaveChangesAsync();

            await Clients.Group(groupId.ToString()).SendAsync("ReceiveGroupMessage", groupMessage);
        }
        public async Task SendMessage(string message)
        {
            var senderId = GetUserIdFromContext();
            if (!senderId.HasValue) return;

            await Clients.All.SendAsync("ReceiveMessage", new
            {
                SenderId = senderId.Value,
                Content = message
            });
        }

    }
}
