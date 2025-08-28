﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.Models;

[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly DatabaseContext _context;
    public GroupController(DatabaseContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return Ok(group);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .Include(g => g.Messages).ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();
        return Ok(group);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] int userId)
    {
        if (!await _context.GroupMembers.AnyAsync(m => m.GroupId == id && m.UserId == userId))
        {
            _context.GroupMembers.Add(new GroupMember { GroupId = id, UserId = userId });
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var member = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
        if (member != null)
        {
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var messages = await _context.GroupMessages
            .Include(m => m.Sender)
            .Where(m => m.GroupId == id)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] GroupMessage message)
    {
        message.GroupId = id;
        message.Timestamp = DateTime.UtcNow;
        _context.GroupMessages.Add(message);
        await _context.SaveChangesAsync();
        return Ok(message);
    }
}
