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
                .OrderBy(c => c.Timestamp)
                .Select(c => new ChatWithUserDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    Timestamp = c.Timestamp,
                    SenderId = c.SenderId,
                    SenderName = _context.Users
                        .Where(u => u.Id == c.SenderId)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Unknown"
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
    }
}
