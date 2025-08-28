using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.DTOs;
using Void.Models;

[ApiController]
[Route("api/chats")]
public class ChatController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ChatController(DatabaseContext context) => _context = context;
    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation(int user1, int user2)
    {
        // Get all messages between the two users
        var chats = await _context.Chats
            .Include(c => c.Sender)
            .Include(c => c.Receiver)
            .Where(c => (c.SenderId == user1 && c.ReceiverId == user2) ||
                        (c.SenderId == user2 && c.ReceiverId == user1))
            .OrderBy(c => c.Timestamp)
            .ToListAsync();

        // Mark unread messages as read for the current user
        var unread = chats.Where(c => c.ReceiverId == user1 && !c.IsRead.GetValueOrDefault()).ToList();
        foreach (var chat in unread)
            chat.IsRead = true;

        await _context.SaveChangesAsync();

        // Return DTOs
        var chatDtos = chats.Select(c => new ChatWithUserDTO
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

        return Ok(chatDtos);
    }




    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatCreateDTO chatDto)
    {
        var chat = new Chat
        {
            SenderId = chatDto.SenderId,
            ReceiverId = chatDto.ReceiverId,
            Content = chatDto.Content,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        var fullChat = await _context.Chats
            .Include(c => c.Sender)
            .Include(c => c.Receiver)
            .FirstOrDefaultAsync(c => c.Id == chat.Id);

        if (fullChat == null)
            return NotFound();

        var result = new ChatWithUserDTO
        {
            Id = fullChat.Id,
            Content = fullChat.Content,
            Timestamp = fullChat.Timestamp ?? DateTime.UtcNow,
            SenderId = fullChat.SenderId ?? 0,
            SenderName = fullChat.Sender?.UserName ?? "Unknown",
            ReceiverId = fullChat.ReceiverId ?? 0,
            ReceiverName = fullChat.Receiver?.UserName ?? "Unknown",
            IsRead = fullChat.IsRead ?? false
        };

        return Ok(result);
    }






}
