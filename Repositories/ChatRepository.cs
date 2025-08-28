using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.DTOs;
using Void.Models;

namespace Void.Repositories
{
    public class ChatRepository
    {
        private readonly DatabaseContext _context;

        public ChatRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<ChatWithUserDTO>> GetAllWithUserAsync()
        {
            return await _context.Chats
                .Include(c => c.Sender)    // Eager load sender
                .Include(c => c.Receiver)  // Eager load receiver
                .OrderBy(c => c.Timestamp)
                .Select(c => new ChatWithUserDTO
                {
                    Id = c.Id,
                    Content = c.Content ?? string.Empty,
                    Timestamp = c.Timestamp ?? DateTime.UtcNow,
                    SenderId = c.SenderId ?? 0,
                    SenderName = c.Sender != null ? c.Sender.UserName : "Unknown",
                    ReceiverId = c.ReceiverId ?? 0,
                    ReceiverName = c.Receiver != null ? c.Receiver.UserName : "Unknown",
                    IsRead = c.IsRead ?? false
                })
                .ToListAsync();
        }

        public async Task<Chat?> GetByIdAsync(int id)
        {
            return await _context.Chats.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var chat = await GetByIdAsync(id);
            if (chat == null) return false;

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAsync(Chat chat)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();
        }
    }
}
