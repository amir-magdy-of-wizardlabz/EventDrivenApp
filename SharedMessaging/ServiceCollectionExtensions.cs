using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SharedEvents;

namespace SharedMessaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // One connection per process, opened on first use and closed by the container.
            services.AddSingleton<IConnection>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    Port = options.Port,
                    UserName = options.UserName,
                    Password = options.Password,
                    DispatchConsumersAsync = true,
                    ClientProvidedName = provider.GetRequiredService<IHostEnvironment>().ApplicationName
                };
                return factory.CreateConnection();
            });

            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
            return services;
        }

        public static IServiceCollection AddRabbitMqConsumer(this IServiceCollection services, string queueName, Action<RabbitMqConsumerBuilder> configure)
        {
            var builder = new RabbitMqConsumerBuilder(services);
            configure(builder);

            services.AddSingleton(new RabbitMqConsumerOptions(queueName, builder.Subscriptions));
            services.AddHostedService<RabbitMqConsumer>();
            return services;
        }
    }
}
