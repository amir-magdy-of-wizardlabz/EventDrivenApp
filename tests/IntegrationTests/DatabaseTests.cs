using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Data;
using OrderService.Infrastructure.Data;
using Testcontainers.PostgreSql;
using UserService.Core.Entities;
using UserService.Core.Exceptions;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace IntegrationTests
{
    // Runs against a real PostgreSQL because the things under test are the migrations and a database constraint.
    public class DatabaseTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:15").Build();

        public async ValueTask InitializeAsync() => await _postgres.StartAsync();

        public async ValueTask DisposeAsync() => await _postgres.DisposeAsync();

        private DbContextOptions<TContext> Options<TContext>() where TContext : DbContext =>
            new DbContextOptionsBuilder<TContext>()
                .UseNpgsql($"{_postgres.GetConnectionString()};Database={typeof(TContext).Name}")
                .Options;

        [Fact]
        public async Task Every_service_can_create_its_schema_from_its_migrations()
        {
            var token = TestContext.Current.CancellationToken;

            await using var users = new UserDbContext(Options<UserDbContext>());
            await using var orders = new OrderDbContext(Options<OrderDbContext>());
            await using var notifications = new NotificationDbContext(Options<NotificationDbContext>());

            await users.Database.MigrateAsync(token);
            await orders.Database.MigrateAsync(token);
            await notifications.Database.MigrateAsync(token);

            Assert.Empty(await users.Database.GetPendingMigrationsAsync(token));
            Assert.Empty(await orders.Database.GetPendingMigrationsAsync(token));
            Assert.Empty(await notifications.Database.GetPendingMigrationsAsync(token));
        }

        [Fact]
        public async Task The_database_rejects_a_second_user_with_the_same_email()
        {
            var token = TestContext.Current.CancellationToken;
            await using (var setup = new UserDbContext(Options<UserDbContext>()))
            {
                await setup.Database.MigrateAsync(token);
            }

            await using var first = new UserDbContext(Options<UserDbContext>());
            await new UserRepository(first).AddUserAsync(new User { Name = "Ann", Email = "ann@example.com" });

            await using var second = new UserDbContext(Options<UserDbContext>());
            await Assert.ThrowsAsync<DuplicateEmailException>(() =>
                new UserRepository(second).AddUserAsync(new User { Name = "Other", Email = "ann@example.com" }));
        }

        [Fact]
        public async Task Deleting_a_user_in_the_order_database_deletes_their_orders()
        {
            var token = TestContext.Current.CancellationToken;
            await using (var setup = new OrderDbContext(Options<OrderDbContext>()))
            {
                await setup.Database.MigrateAsync(token);
                setup.Users.Add(new OrderService.Core.Entities.User { Id = 5 });
                setup.Orders.Add(new OrderService.Core.Entities.Order
                {
                    UserId = 5,
                    ProductName = "Laptop",
                    Quantity = 1,
                    Price = 1500m,
                    OrderDate = DateTime.UtcNow
                });
                await setup.SaveChangesAsync(token);
            }

            await using (var delete = new OrderDbContext(Options<OrderDbContext>()))
            {
                delete.Users.Remove(await delete.Users.SingleAsync(token));
                await delete.SaveChangesAsync(token);
            }

            await using var verify = new OrderDbContext(Options<OrderDbContext>());
            Assert.Empty(await verify.Orders.ToListAsync(token));
        }
    }
}
