namespace OldIsGold.DAL.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int Score { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime RatingDate { get; set; } = DateTime.Now;

        // Foreign Keys
        public required string RatedUserId { get; set; }
        public required string RaterUserId { get; set; }
        public int OrderId { get; set; }

        // Navigation properties
        public virtual ApplicationUser RatedUser { get; set; } = null!;
        public virtual ApplicationUser RaterUser { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}
