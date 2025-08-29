using Microsoft.AspNetCore.SignalR;
using Void.DTOs;
using Void.Hubs;
using Void.Models;
using Void.Repositories;

namespace Void.Services
{
    public class ChatService
    {
        private readonly ChatRepository _repository;
        private readonly IHubContext<PrivateChatHub> _hub;

        public ChatService(ChatRepository repository, IHubContext<PrivateChatHub> hub)
        {
            _repository = repository;
            _hub = hub;
        }

        public async Task<List<ChatWithUserDTO>> GetConversationAsync(int user1, int user2)
        {
            var chats = await _repository.GetConversationAsync(user1, user2);

            var unread = chats.Where(c => c.ReceiverId == user1 && !c.IsRead.GetValueOrDefault()).ToList();
            foreach (var chat in unread) chat.IsRead = true;
            await Task.WhenAll(unread.Select(c => _repository.UpdateAsync(c)));

            return chats.Select(c => new ChatWithUserDTO
            {
                Id = c.Id,
                Content = c.Content ?? "",
                Timestamp = c.Timestamp ?? DateTime.UtcNow,
                SenderId = c.SenderId ?? 0,
                SenderName = c.Sender?.UserName ?? "Unknown",
                ReceiverId = c.ReceiverId ?? 0,
                ReceiverName = c.Receiver?.UserName ?? "Unknown",
                IsRead = c.IsRead ?? false
            }).ToList();
        }

        public async Task<ChatWithUserDTO> SendMessageAsync(ChatCreateDTO chatDto)
        {
            var chat = new Chat
            {
                SenderId = chatDto.SenderId,
                ReceiverId = chatDto.ReceiverId,
                Content = chatDto.Content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await _repository.AddAsync(chat);

            var result = new ChatWithUserDTO
            {
                Id = chat.Id,
                Content = chat.Content,
                Timestamp = chat.Timestamp ?? DateTime.UtcNow,
                SenderId = chat.SenderId ?? 0,
                SenderName = chat.Sender?.UserName ?? "Unknown",
                ReceiverId = chat.ReceiverId ?? 0,
                ReceiverName = chat.Receiver?.UserName ?? "Unknown",
                IsRead = chat.IsRead ?? false
            };

            await _hub.Clients.User(chat.ReceiverId.ToString()).SendAsync("ReceiveMessage", result);

            return result;
        }
    }
}
