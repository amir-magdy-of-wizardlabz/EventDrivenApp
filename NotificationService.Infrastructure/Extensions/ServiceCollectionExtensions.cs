using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Utilities;
using SharedEvents;
using SharedEvents.Events;
using SharedMessaging;

namespace NotificationService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string QueueName = "notification-service";

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataInfrastructureService(services, configuration);
            AddMessagingInfrastructureServices(services, configuration);
            AddNotificationInfrastructureServices(services);
            return services;
        }

        private static void AddDataInfrastructureService(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHostedService<MigrationHostedService>();
        }

        private static void AddNotificationInfrastructureServices(IServiceCollection services)
        {
            services.AddScoped<INotificationService, EmailSender>();
        }

        private static void AddMessagingInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRabbitMq(configuration);

            // One queue for the whole service keeps a user's events in the order they were published.
            services.AddRabbitMqConsumer(QueueName, consumer => consumer
                .Subscribe<UserCreatedEvent, UserCreatedEventHandler>(Exchanges.Users, RoutingKeys.UserCreated)
                .Subscribe<UserUpdatedEvent, UserUpdatedEventHandler>(Exchanges.Users, RoutingKeys.UserUpdated)
                .Subscribe<UserDeletedEvent, UserDeletedEventHandler>(Exchanges.Users, RoutingKeys.UserDeleted)
                .Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>(Exchanges.Orders, RoutingKeys.OrderCreated));
        }
    }
}
