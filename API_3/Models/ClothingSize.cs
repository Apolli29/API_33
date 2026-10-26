using System.ComponentModel.DataAnnotations;
namespace API_3.Models
{
    public class ClothingSize
    {
        public int Id { get; set; }
        public int ClothingId { get; set; }
        public Clothing Clothing { get; set; } = null!;
        [Required] public string SizeLetter { get; set; } = string.Empty;
        public int SizeNumber { get; set; }
        public int QuantityInStock { get; set; }
    }
}
