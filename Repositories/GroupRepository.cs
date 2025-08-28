﻿using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.Models;

namespace Void.Repositories
{
    public class GroupRepository
    {
        private readonly DatabaseContext _context;

        public GroupRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Group> CreateGroupAsync(string groupName, List<int> userIds)
        {
            var group = new Group { Name = groupName };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            foreach (var userId in userIds)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = group.Id,
                    UserId = userId
                });
            }
            await _context.SaveChangesAsync();

            return group;
        }

        public async Task<Group?> GetGroupAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task AddMemberAsync(int groupId, int userId)
        {
            if (!await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId))
            {
                _context.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveMemberAsync(int groupId, int userId)
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (member != null)
            {
                _context.GroupMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<GroupMessage> AddMessageAsync(int groupId, int senderId, string content)
        {
            var message = new GroupMessage
            {
                GroupId = groupId,
                SenderId = senderId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };
            _context.GroupMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<GroupMessage>> GetMessagesAsync(int groupId)
        {
            return await _context.GroupMessages
                .Include(m => m.Sender)
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
