namespace Void.DTOs
{
    public class GroupMessageDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public int GroupId { get; set; }
    }
}
