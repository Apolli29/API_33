using System.ComponentModel.DataAnnotations;
namespace API_3.Models
{
    public class Clothing
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int PurchaseCount { get; set; }
        public string? Type { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<ClothingSize> Sizes { get; set; } = new();
    }
}
