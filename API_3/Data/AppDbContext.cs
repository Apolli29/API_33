using Microsoft.EntityFrameworkCore;
using API_3.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace API_3.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Clothing> Clothings { get; set; }
    public DbSet<ClothingSize> ClothingSizes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.ClothingSize).WithMany().HasForeignKey(oi => oi.ClothingSizeId).OnDelete(DeleteBehavior.Restrict);
    }
}
