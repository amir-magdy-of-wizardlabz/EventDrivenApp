using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace IntegrationTests
{
    // One broker for all messaging tests; each test uses its own exchange and queue names.
    public class RabbitMqFixture : IAsyncLifetime
    {
        public const string UserName = "test";
        public const string Password = "test";

        private readonly RabbitMqContainer _container = new RabbitMqBuilder("rabbitmq:3-management")
            .WithUsername(UserName)
            .WithPassword(Password)
            .Build();

        private IConnection? _connection;

        public string HostName => _container.Hostname;
        public int Port => _container.GetMappedPublicPort(5672);

        // A plain connection the tests use to look at the broker from outside the code under test.
        public IConnection Connection => _connection ?? throw new InvalidOperationException("The fixture is not initialized.");

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();
            _connection = new ConnectionFactory { HostName = HostName, Port = Port, UserName = UserName, Password = Password }
                .CreateConnection();
        }

        public async ValueTask DisposeAsync()
        {
            _connection?.Dispose();
            await _container.DisposeAsync();
        }
    }
}
