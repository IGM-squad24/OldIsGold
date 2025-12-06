namespace OldIsGold.DAL.Models
{
    public class ItemImage
    {
        public int ImageId { get; set; }
        public int ItemId { get; set; }
        public required byte[] ImageData { get; set; }
        public required string ContentType { get; set; }
        public required string FileName { get; set; }
        public bool IsMain { get; set; } = false;

        // Navigation property
        public virtual Item Item { get; set; } = null!;
    }
}
