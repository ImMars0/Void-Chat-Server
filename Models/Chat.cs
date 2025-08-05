namespace Void.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverID { get; set; }
    }



}