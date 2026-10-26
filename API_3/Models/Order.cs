namespace API_3.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new();
    }
}
