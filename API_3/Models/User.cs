using System.ComponentModel.DataAnnotations;
namespace API_3.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        [Required] public string Login { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.RegularUser;
        public List<Order> Orders { get; set; } = new();
    }
}
