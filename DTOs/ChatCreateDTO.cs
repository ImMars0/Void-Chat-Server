namespace Void.DTOs
{
    public class ChatCreateDTO
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

}
