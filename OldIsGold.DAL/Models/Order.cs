namespace OldIsGold.DAL.Models
{
    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public class Order
    {
       public int OrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string? PaymentMethod { get; set; } // Dummy payment

        // Foreign Keys
        public int ItemId { get; set; }
        public required string BuyerId { get; set; }
        public required string SellerId { get; set; }

        // Navigation properties
        public virtual Item Item { get; set; } = null!;
        public virtual ApplicationUser Buyer { get; set; } = null!;
        public virtual ApplicationUser Seller { get; set; } = null!;
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
