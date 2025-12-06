namespace OldIsGold.DAL.Models
{
    public enum ReportStatus
    {
        Pending,
        Reviewed,
        Resolved
    }

    public class Report
    {
        public int ReportId { get; set; }
        public required string Reason { get; set; }
        public string? Description { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        // Foreign Keys
        public int ItemId { get; set; }
        public required string ReporterId { get; set; }
        public string? ReviewedByAdminId { get; set; }

        // Navigation properties
        public virtual Item Item { get; set; } = null!;
        public virtual ApplicationUser Reporter { get; set; } = null!;
        public virtual ApplicationUser? ReviewedByAdmin { get; set; }
    }
}
