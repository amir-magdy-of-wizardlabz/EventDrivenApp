using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Core.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddDataInfrastructureService(services, configuration);
            AddMesagingInfrastructureServices(services, configuration);
            return services;
        }

        private static void AddDataInfrastructureService(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHostedService<MigrationHostedService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
        }

        private static IServiceCollection AddMesagingInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<OrderRabbitMqSetupService>();            
            services.AddHostedService<UserCreatedEventHandler>(provider =>
            {
                return new UserCreatedEventHandler(provider, configuration);
            });
            services.AddScoped<IOrderPublisher, OrderService.Infrastructure.Messaging.OrderServicePublisher>();
            return services;
        }
    }
}
