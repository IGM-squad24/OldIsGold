using Microsoft.AspNetCore.Identity;

namespace OldIsGold.DAL.Models
{
    public enum UserType
    {
        Seller,
        Buyer
    }

    public class ApplicationUser : IdentityUser
    {
        public required UserType UserType { get; set; }
        public required string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public double Rating { get; set; } = 0.0;

        // Navigation properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
        public virtual ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Wishlist> WishlistItems { get; set; } = new List<Wishlist>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
        public virtual ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
        public virtual ICollection<AdminLog> AdminLogs { get; set; } = new List<AdminLog>();
    }
}
