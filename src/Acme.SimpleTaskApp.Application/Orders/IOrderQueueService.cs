using Acme.SimpleTaskApp.Orders.Dtos;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders
{
    public interface IOrderQueueService
    {
        Task<bool> TryLockProductsAsync(CreateOrderInput orderInput);
        Task ReleaseProductLocksAsync(CreateOrderInput orderInput);
        Task ProcessOrderAsync(CreateOrderInput orderInput);
    }
}