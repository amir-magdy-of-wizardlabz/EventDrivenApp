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
            // User ids are assigned by UserService and copied from its events.
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Order>()
                .HasOne<User>()  // Configure the relationship
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Deleting a user removes their orders

            base.OnModelCreating(modelBuilder);
        }
    }
}
