using System.ComponentModel.DataAnnotations;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Models
{
    public class CreateItemViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        public ItemCondition Condition { get; set; }

        [Range(1500, 2100)]
        public int? Year { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }

    public class EditItemViewModel
    {
        public int ItemId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        public ItemCondition Condition { get; set; }

        [Range(1500, 2100)]
        public int? Year { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }
}
