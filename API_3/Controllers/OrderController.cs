using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_3.Data;
using API_3.Models;
using Microsoft.AspNetCore.Authorization;

namespace API_3.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrderController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = new Order { 
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemReq in request.Items)
        {
            var size = await _db.ClothingSizes.Include(cs => cs.Clothing).FirstOrDefaultAsync(cs => cs.Id == itemReq.ClothingSizeId);
            if (size == null || size.QuantityInStock < itemReq.Quantity)
                return BadRequest($"Недостаточно товара для размера ID {itemReq.ClothingSizeId}");

            size.QuantityInStock -= itemReq.Quantity;
            size.Clothing.PurchaseCount += itemReq.Quantity;
            order.Items.Add(new OrderItem { 
                ClothingSizeId = itemReq.ClothingSizeId,
                Quantity = itemReq.Quantity,
                PriceAtPurchase = size.Clothing.Price 
            });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyOrders), new { id = order.Id }, order);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _db.Orders.Include(o => o.Items).ThenInclude(i => i.ClothingSize).Where(o => o.UserId == userId).ToListAsync());
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
    {
        return Ok(await _db.Orders.Include(o => o.User).Include(o => o.Items).ThenInclude(i => i.ClothingSize).Where(o => o.Status == status).ToListAsync());
    }

    [HttpGet("buyer/{userId}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetBuyerOrders(int userId)
    {
        return Ok(await _db.Orders.Include(o => o.Items).ThenInclude(i => i.ClothingSize).Where(o => o.UserId == userId).ToListAsync());
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = status;
        await _db.SaveChangesAsync();
        return Ok(order);
    }
}
public class CreateOrderRequest { 
    public List<OrderItemRequest> Items { get; set; } = new();
}
public class OrderItemRequest {
    public int ClothingSizeId { get; set; }
    public int Quantity { get; set; }
}