using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;

namespace OrderService.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne<User>()  // Configure the relationship
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Optional: define delete behavior

            base.OnModelCreating(modelBuilder);
        }
    }
}
