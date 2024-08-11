using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IEventPublisher
    {
        void Publish(string queueName, User user);
    }
}
