using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Core.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;
using SharedEvents;
using SharedEvents.Events;
using SharedMessaging;

namespace OrderService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string QueueName = "order-service";

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataInfrastructureService(services, configuration);
            AddMessagingInfrastructureServices(services, configuration);
            return services;
        }

        private static void AddDataInfrastructureService(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHostedService<MigrationHostedService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
        }

        private static void AddMessagingInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRabbitMq(configuration);

            // One queue for the whole service keeps a user's events in the order they were published.
            services.AddRabbitMqConsumer(QueueName, consumer => consumer
                .Subscribe<UserCreatedEvent, UserCreatedEventHandler>(Exchanges.Users, RoutingKeys.UserCreated)
                .Subscribe<UserDeletedEvent, UserDeletedEventHandler>(Exchanges.Users, RoutingKeys.UserDeleted));
        }
    }
}
