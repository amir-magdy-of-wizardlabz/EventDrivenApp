namespace UserService.Core.Interfaces
{
    public interface IEventPublisher
    {
        void Publish(string queueName, string message);
    }
}
