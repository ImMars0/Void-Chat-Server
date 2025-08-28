using Microsoft.AspNetCore.SignalR;
using Void.Database;
using Void.Models;

namespace Void.Hubs
{
    public class GroupChatHub : Hub
    {
        private readonly DatabaseContext _context;

        public GroupChatHub(DatabaseContext context)
        {
            _context = context;
        }

        public async Task JoinGroup(int groupId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());

        public async Task LeaveGroup(int groupId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());

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

        private int? GetUserIdFromContext()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userIdStr))
                if (int.TryParse(userIdStr, out var userId)) return userId;
            return null;
        }
    }
}
