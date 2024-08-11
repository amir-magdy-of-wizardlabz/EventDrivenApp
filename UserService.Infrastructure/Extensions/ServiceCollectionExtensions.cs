using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            // Configure DbContext with PostgreSQL
            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Apply migrations at startup
            services.AddHostedService<MigrationHostedService>();

            return services;
        }
    }
}
