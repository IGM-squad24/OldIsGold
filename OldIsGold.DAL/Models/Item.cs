namespace OldIsGold.DAL.Models
{
    public enum ItemCondition
    {
        New,
        Used,
        Vintage
    }

    public enum ItemStatus
    {
        Pending,
        Approved,
        Rejected,
        Sold
    }

    public class Item
    {
        public int ItemId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public ItemCondition Condition { get; set; }
        public int? Year { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Pending;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public int ViewCount { get; set; } = 0;

        // Foreign Keys
        public int CategoryId { get; set; }
        public required string SellerId { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ApplicationUser Seller { get; set; } = null!;
        public virtual ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Wishlist> WishlistedBy { get; set; } = new List<Wishlist>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
