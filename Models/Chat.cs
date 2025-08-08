namespace Void.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public string? Content { get; set; } = null!;

        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

        public int? SenderId { get; set; }

        public int? ReceiverId { get; set; }


        public bool? IsRead { get; set; } = false;

        // public string? SenderName { get; set; } = null;
        // public string? ReceiverName { get; set; } = null!;
    }
}
