using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataInfrastructureService(services, configuration);
            AddMessagingInfrastructureServices(services, configuration);
            return services;
        }

        private static void AddDataInfrastructureService(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<NotificationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHostedService<MigrationHostedService>();

        }

        private static IServiceCollection AddMessagingInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register messaging related services here.
            return services;
        }
    }
}
