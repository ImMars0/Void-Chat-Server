using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.Models;

namespace Void.Repositories
{
    public class FriendshipRepository
    {
        private readonly DatabaseContext _context;

        public FriendshipRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Friendship> GetByIdAsync(int id)
        {
            return await _context.Friendships
                .Include(f => f.Friend)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Friendship> GetByUsersAsync(int userId, int friendId)
        {
            return await _context.Friendships
                .Include(f => f.Friend)
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));
        }

        public async Task<List<Friendship>> GetUserFriendshipsAsync(int userId)
        {
            return await _context.Friendships
                .Include(f => f.Friend)
                .Where(f => (f.UserId == userId || f.FriendId == userId) &&
                           f.Status == FriendshipStatus.Accepted)
                .ToListAsync();
        }

        public async Task<List<Friendship>> GetPendingRequestsAsync(int userId)
        {
            return await _context.Friendships
                .Include(f => f.User)
                .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
                .ToListAsync();
        }

        public async Task AddAsync(Friendship friendship)
        {
            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Friendship friendship)
        {
            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var friendship = await GetByIdAsync(id);
            if (friendship == null) return false;

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AreFriends(int userId, int friendId)
        {
            return await _context.Friendships
                .AnyAsync(f =>
                    ((f.UserId == userId && f.FriendId == friendId) ||
                     (f.UserId == friendId && f.FriendId == userId)) &&
                    f.Status == FriendshipStatus.Accepted);
        }
    }
}
