using OrderService.Core.Entities;
using System.Threading.Tasks;

namespace OrderService.Core.Interfaces
{
    public interface IOrderPublisher
    {
        Task CreateOrder(SharedEvents.Events.OrderCreatedEvent order);
    }
}
