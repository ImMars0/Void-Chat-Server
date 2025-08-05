using Void.Database;
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
        public List<Chat> GetAll()
        {
            return _context.Chats.ToList();
        }
        public Chat? GetById(int id)
        {
            return _context.Chats.FirstOrDefault(c => c.Id == id);
        }
        public void Add(Chat chat)
        {
            _context.Chats.Add(chat);
            _context.SaveChanges();
        }
        public bool Delete(int id)
        {
            var chat = GetById(id);
            if (chat == null) return false;
            _context.Chats.Remove(chat);
            _context.SaveChanges();
            return true;
        }
    }
}
