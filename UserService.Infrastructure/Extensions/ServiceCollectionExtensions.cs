using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataUserServices(services, configuration);
            AddEventPublisher(services, configuration);
            return services;
        }

        private static void AddDataUserServices(IServiceCollection services,IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            // Configure DbContext with PostgreSQL
            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Apply migrations at startup
            services.AddHostedService<MigrationHostedService>();
        }

        private static IServiceCollection AddEventPublisher(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMQHostName = configuration["RabbitMQ:HostName"];
            var rabbitMQUserName = configuration["RabbitMQ:UserName"];
            var rabbitMQPassword = configuration["RabbitMQ:Password"];

            if (string.IsNullOrEmpty(rabbitMQHostName))
                throw new ArgumentNullException(nameof(rabbitMQHostName), "RabbitMQ hostname cannot be null or empty.");

            if (string.IsNullOrEmpty(rabbitMQUserName))
                throw new ArgumentNullException(nameof(rabbitMQUserName), "RabbitMQ username cannot be null or empty.");

            if (string.IsNullOrEmpty(rabbitMQPassword))
                throw new ArgumentNullException(nameof(rabbitMQPassword), "RabbitMQ password cannot be null or empty.");


            services.AddSingleton<IEventPublisher>(new EventPublisher(rabbitMQHostName, rabbitMQUserName, rabbitMQPassword));
            
            services.AddHostedService<RabbitMqSetupService>();

            return services;
        }
    }
}
