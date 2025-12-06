namespace OldIsGold.DAL.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public required string Content { get; set; }
        public DateTime SentDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        // Foreign Keys
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public int? ItemId { get; set; } // Optional - for context

        // Navigation properties
        public virtual ApplicationUser Sender { get; set; } = null!;
        public virtual ApplicationUser Receiver { get; set; } = null!;
        public virtual Item? Item { get; set; }
    }
}
