using Microsoft.AspNetCore.Mvc;
using Void.DTOs;
using Void.Services;

[ApiController]
[Route("api/chats")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation(int user1, int user2)
    {
        var chats = await _chatService.GetConversationAsync(user1, user2);
        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatCreateDTO chatDto)
    {
        var result = await _chatService.SendMessageAsync(chatDto);
        return Ok(result);
    }
}
