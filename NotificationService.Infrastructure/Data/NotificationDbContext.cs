using Microsoft.EntityFrameworkCore;
using NotificationService.Core.Entities;

namespace NotificationService.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
