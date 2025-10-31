using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Abp.Dependency;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Abp.Domain.Repositories;
using System.Threading;
using Acme.SimpleTaskApp.Orders.Dtos;

namespace Acme.SimpleTaskApp.Orders
{
    public class OrderQueueService : IOrderQueueService, ISingletonDependency
    {
        private readonly ConcurrentDictionary<int, SemaphoreSlim> _productLocks = new();
        private readonly ConcurrentQueue<QueuedOrder> _orderQueue = new();
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);
        private bool _isProcessing;

        public OrderQueueService(IRepository<ProductVariant> productVariantRepository)
        {
            _productVariantRepository = productVariantRepository;
            _isProcessing = false;
        }

        private class QueuedOrder
        {
            public CreateOrderInput OrderInput { get; set; }
            public TaskCompletionSource<bool> CompletionSource { get; set; }
        }

        public async Task<bool> TryLockProductsAsync(CreateOrderInput orderInput)
        {
            var queuedOrder = new QueuedOrder
            {
                OrderInput = orderInput,
                CompletionSource = new TaskCompletionSource<bool>()
            };

            // Thêm đơn hàng vào hàng đợi
            _orderQueue.Enqueue(queuedOrder);

            // Bắt đầu xử lý queue nếu chưa có process nào đang chạy
            await StartProcessingQueueAsync();

            // Đợi kết quả xử lý của đơn hàng này
            return await queuedOrder.CompletionSource.Task;
        }

        private async Task StartProcessingQueueAsync()
        {
            // Đảm bảo chỉ một process được chạy tại một thời điểm
            if (await _queueSemaphore.WaitAsync(0))
            {
                try
                {
                    if (_isProcessing)
                        return;

                    _isProcessing = true;
                    await ProcessQueueAsync();
                }
                finally
                {
                    _isProcessing = false;
                    _queueSemaphore.Release();
                }
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (_orderQueue.TryDequeue(out var queuedOrder))
            {
                try
                {
                    // Kiểm tra và khóa sản phẩm
                    var success = await LockAndVerifyStockAsync(queuedOrder.OrderInput);
                    if (success)
                    {
                        await ProcessOrderAsync(queuedOrder.OrderInput);
                        queuedOrder.CompletionSource.SetResult(true);
                    }
                    else
                    {
                        queuedOrder.CompletionSource.SetResult(false);
                    }
                }
                catch (Exception ex)
                {
                    queuedOrder.CompletionSource.SetException(ex);
                }
            }
        }

        private async Task<bool> LockAndVerifyStockAsync(CreateOrderInput orderInput)
        {
            var semaphores = orderInput.OrderDetails
                .Select(od => GetOrCreateLock(od.ProductVariantId))
                .ToList();

            try
            {
                // Cố gắng khóa tất cả sản phẩm trong 10 giây
                var tasks = semaphores.Select(s => s.WaitAsync(TimeSpan.FromSeconds(10)));
                await Task.WhenAll(tasks);

                // Kiểm tra tồn kho
                foreach (var orderDetail in orderInput.OrderDetails)
                {
                    var productVariant = await _productVariantRepository.GetAll()
                        .Where(pv => pv.Id == orderDetail.ProductVariantId)
                        .FirstOrDefaultAsync();

                    if (productVariant == null || productVariant.StockQuantity < orderDetail.Quantity)
                    {
                        await ReleaseProductLocksAsync(orderInput);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                await ReleaseProductLocksAsync(orderInput);
                return false;
            }
        }

        public async Task ReleaseProductLocksAsync(CreateOrderInput orderInput)
        {
            foreach (var orderDetail in orderInput.OrderDetails)
            {
                if (_productLocks.TryGetValue(orderDetail.ProductVariantId, out var semaphore))
                {
                    try
                    {
                        semaphore.Release();
                    }
                    catch (Exception)
                    {
                        // Xử lý trường hợp semaphore đã được giải phóng
                    }
                }
            }
        }

        public async Task ProcessOrderAsync(CreateOrderInput orderInput)
        {
            try
            {
                foreach (var orderDetail in orderInput.OrderDetails)
                {
                    var productVariant = await _productVariantRepository.GetAll()
                        .Where(pv => pv.Id == orderDetail.ProductVariantId)
                        .FirstOrDefaultAsync();

                    if (productVariant != null)
                    {
                        productVariant.StockQuantity -= orderDetail.Quantity;
                        await _productVariantRepository.UpdateAsync(productVariant);
                    }
                }
            }
            finally
            {
                await ReleaseProductLocksAsync(orderInput);
            }
        }

        private SemaphoreSlim GetOrCreateLock(int productVariantId)
        {
            return _productLocks.GetOrAdd(productVariantId, _ => new SemaphoreSlim(1, 1));
        }
    }
}