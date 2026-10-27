using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_3.Data;
using API_3.Models;
using Microsoft.AspNetCore.Authorization;

namespace API_3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClothingController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClothingController(AppDbContext db) => _db = db;
    
    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> CreateClothing([FromBody] CreateClothingRequest request)
    {
        var clothing = new Clothing
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description,
            Type = request.Type,
            IsAvailable = true
        };
        foreach (var sizeReq in request.Sizes)
        {
            clothing.Sizes.Add(new ClothingSize
            {
                SizeLetter = sizeReq.SizeLetter,
                SizeNumber = sizeReq.SizeNumber,
                QuantityInStock = sizeReq.QuantityInStock
            });
        }
        _db.Clothings.Add(clothing);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = clothing.Id }, clothing);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.Clothings.Include(c => c.Sizes).AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(c => c.Name.Contains(search));
        return Ok(await query.Where(c => c.IsAvailable).ToListAsync());
    }

    [HttpPut("{id}/remove-from-sale")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> RemoveFromSale(int id)
    {
        var item = await _db.Clothings.FindAsync(id);
        if (item == null) return NotFound();
        item.IsAvailable = false;
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id}/return-to-sale")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ReturnToSale(int id)
    {
        var item = await _db.Clothings.FindAsync(id);
        if (item == null) return NotFound();
        item.IsAvailable = true;
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("size/{sizeId}/quantity")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> UpdateSizeQuantity(int sizeId, [FromBody] int quantity)
    {
        var size = await _db.ClothingSizes.FindAsync(sizeId);
        if (size == null) return NotFound();
        size.QuantityInStock = quantity;
        await _db.SaveChangesAsync();
        return Ok(size);
    }
    public class CreateClothingRequest
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public List<SizeRequest> Sizes { get; set; } = new();
    }

    public class SizeRequest
    {
        public string SizeLetter { get; set; } = "";
        public int SizeNumber { get; set; }
        public int QuantityInStock { get; set; }
    }
}
