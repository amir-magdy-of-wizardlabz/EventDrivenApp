using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Utilities;

namespace NotificationService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataInfrastructureService(services, configuration);
            AddMessagingInfrastructureServices(services, configuration);
            AddNotificationInfrastructureServices(services, configuration);
            return services;
        }

        private static void AddDataInfrastructureService(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHostedService<MigrationHostedService>();

        }
        private static IServiceCollection AddNotificationInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationService, EmailSender>();
            return services;
        }

        private static IServiceCollection AddMessagingInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<UserCreatedEventHandler>();
            services.AddHostedService<OrderCreatedEventHandler>();
            return services;
        }
    }
}
