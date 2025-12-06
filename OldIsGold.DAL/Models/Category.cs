namespace OldIsGold.DAL.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
