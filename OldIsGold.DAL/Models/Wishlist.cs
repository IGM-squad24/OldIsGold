namespace OldIsGold.DAL.Models
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        public required string UserId { get; set; }
        public int ItemId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Item Item { get; set; } = null!;
    }
}
