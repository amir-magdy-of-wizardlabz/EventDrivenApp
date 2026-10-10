using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedMessaging;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataUserServices(services, configuration);

            // This service only publishes; the queues belong to the services that consume.
            services.AddRabbitMq(configuration);

            return services;
        }

        private static void AddDataUserServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            // Configure DbContext with PostgreSQL
            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Apply migrations at startup
            services.AddHostedService<MigrationHostedService>();
        }
    }
}
