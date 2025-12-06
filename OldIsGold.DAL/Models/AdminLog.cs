namespace OldIsGold.DAL.Models
{
    public class AdminLog
    {
        public int LogId { get; set; }
        public required string Action { get; set; }
        public required string TargetType { get; set; }
        public required string TargetId { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        // Foreign Key
        public required string AdminId { get; set; }

        // Navigation property
        public virtual ApplicationUser Admin { get; set; } = null!;
    }
}
