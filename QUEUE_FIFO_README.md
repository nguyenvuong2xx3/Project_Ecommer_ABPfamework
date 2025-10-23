# H? Th?ng Queue FIFO Cho ??t Hàng

## T?i Sao S? D?ng Queue?

### So Sánh V?i Gi?i Pháp C? (SemaphoreSlim)

| Tiêu Chí | SemaphoreSlim (C?) | Queue FIFO (M?i) |
|----------|-------------------|------------------|
| **Th? t? x? lý** | ? Không ??m b?o (race condition) | ? First In First Out |
| **Công b?ng** | ? Ng??i sau có th? ???c x? lý tr??c | ? Ng??i tr??c ???c ?u tiên |
| **Tr?i nghi?m** | ? User không bi?t v? trí c?a mình | ? Bi?t v? trí và th?i gian ch? |
| **Scalability** | ? Ch? ho?t ??ng trong 1 server | ? Có th? m? r?ng v?i Redis/RabbitMQ |
| **Complexity** | ? ??n gi?n | ?? Ph?c t?p h?n m?t chút |

## Ki?n Trúc

### 1. OrderQueueService
Service qu?n lý queue cho m?i ProductVariant:

```csharp
public class OrderQueueService : ISingletonDependency
{
    // Dictionary: ProductVariantId -> Queue<OrderRequest>
    private readonly ConcurrentDictionary<int, ConcurrentQueue<OrderRequest>> _productQueues;
    
    // Thêm request vào queue
    public async Task<OrderQueueTicket> EnqueueOrderRequest(...)
    
    // X? lý queue theo th? t? FIFO
    private async Task ProcessQueueAsync(int productVariantId)
}
```

### 2. OrderQueueTicket
Tracking tr?ng thái c?a m?i order request:

```csharp
public class OrderQueueTicket
{
    public string TicketId { get; set; }          // ID unique
    public int Position { get; set; }             // V? trí trong hàng
    public OrderQueueStatus Status { get; set; }  // Waiting/Processing/Completed/Failed
    public int EstimatedWaitTimeSeconds { get; } // Th?i gian ch? ??c tính
}
```

### 3. OrderQueueStatus
```csharp
public enum OrderQueueStatus
{
    Waiting = 0,      // ?ang ch? trong queue
    Processing = 1,   // ?ang ???c x? lý
    Completed = 2,    // Hoàn thành
    Failed = 3        // Th?t b?i (h?t hàng, l?i, v.v.)
}
```

## Workflow Chi Ti?t

### K?ch B?n: 3 User ??t Cùng 1 S?n Ph?m (Stock = 2)

```
Timeline:
---------
T0: User A click "??t hàng" ? Thêm vào Queue (Position 1)
T0: User B click "??t hàng" ? Thêm vào Queue (Position 2)  
T0: User C click "??t hàng" ? Thêm vào Queue (Position 3)

T1: Queue b?t ??u x? lý User A
    - Check stock: 2 ? 1 ?
    - Tr? stock: 2 ? 1
    - T?o order thành công
    - Status: Completed

T2: Queue x? lý User B (t? ??ng, FIFO)
    - Check stock: 1 ? 1 ?
    - Tr? stock: 1 ? 0
    - T?o order thành công
    - Status: Completed

T3: Queue x? lý User C
    - Check stock: 0 < 1 ?
    - Throw exception: "Ch? còn 0 s?n ph?m"
    - Status: Failed

K?t qu?:
--------
? User A: Thành công (Position 1, x? lý ??u tiên)
? User B: Thành công (Position 2, x? lý sau A)
? User C: Th?t b?i (Position 3, h?t hàng)
```

## Code Flow

### 1. Client Submit Order

```javascript
// Cart/Index.js
$('form[action*="CreateOrder"]').on('submit', function (e) {
    e.preventDefault();
    
    // Hi?n th? "?ang x? lý..."
    var $loadingMessage = abp.message.info(
        '??n hàng c?a b?n ?ang ???c x? lý...',
        '?ang x? lý'
    );
    
    $.ajax({
        url: '/Orders/CreateOrder',
        type: 'POST',
        data: { PaymentMethod: paymentMethod },
        success: function (result) {
            // Thành công ? Redirect
            window.location.href = '/Orders/IndexForCustomer';
        },
        error: function (xhr) {
            // Hi?n th? l?i (ví d?: h?t hàng)
            var errorMessage = xhr.responseJSON.error.message;
            abp.message.error(errorMessage);
        }
    });
});
```

### 2. Server: OrdersController

```csharp
[HttpPost]
public async Task<IActionResult> CreateOrder(int PaymentMethod)
{
    try
    {
        var currentUserId = AbpSession.UserId ?? 
            throw new UserFriendlyException("Vui lòng ??ng nh?p");
        
        var getCart = await _cartAppService.GetCart();
        
        var orderDetails = getCart.CartItems.Select(cartItem => new OrderDetailDto
        {
            ProductId = cartItem.IdProductVariant,
            Quantity = cartItem.Quantity
        }).ToList();

        // G?i CreateOrder v?i Queue
        var orderId = await _ordersAppService.CreateOrder(new CreateOrderInput
        {
            UserId = currentUserId,
            OrderDetails = orderDetails,
            PaymentMethod = PaymentMethod,
            Status = 0
        });

        await _cartAppService.DeleteCart(currentUserId);
        
        return Ok(new { success = true, orderId });
    }
    catch (UserFriendlyException ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}
```

### 3. Server: OrdersAppService v?i Queue

```csharp
public async Task<int> CreateOrder(CreateOrderInput input)
{
    // Validate và tính t?ng giá tr??c
    decimal totalPrice = 0;
    var orderDetailsList = new List<OrderDetails>();
    
    foreach (var item in input.OrderDetails)
    {
        var productVariant = await _productVariantRepository
            .GetAll()
            .Where(pv => pv.Id == item.ProductId)
            .FirstOrDefaultAsync();

        totalPrice += item.Quantity * productVariant.Price;
        orderDetailsList.Add(new OrderDetails { ... });
    }

    // Thêm vào Queue theo th? t? ProductVariantId
    var sortedProductIds = input.OrderDetails
        .Select(x => x.ProductId)
        .OrderBy(x => x)
        .Distinct()
        .ToList();

    int orderId = 0;
    
    foreach (var productId in sortedProductIds)
    {
        var orderDetail = orderDetailsList.First(od => od.ProductVariantId == productId);
        
        // Enqueue request
        var ticket = await _orderQueueService.EnqueueOrderRequest(
            productId,
            orderDetail.Quantity,
            async () =>
            {
                // Critical Section: X? lý trong queue
                var productVariant = await _productVariantRepository.GetAsync(productId);
                
                // Ki?m tra stock
                if (productVariant.StockQuantity < orderDetail.Quantity)
                {
                    throw new UserFriendlyException(
                        $"S?n ph?m ch? còn {productVariant.StockQuantity} s?n ph?m");
                }

                // T?o order (l?n ??u)
                if (orderId == 0)
                {
                    Order order = new Order { ... };
                    orderId = await _ordersRepository.InsertAndGetIdAsync(order);
                }

                // T?o OrderDetail
                orderDetail.OrderId = orderId;
                await _orderDetailsRepository.InsertAsync(orderDetail);

                // Tr? stock
                productVariant.StockQuantity -= orderDetail.Quantity;
                await _productVariantRepository.UpdateAsync(productVariant);

                await CurrentUnitOfWork.SaveChangesAsync();
            });

        // Ch? ticket ???c x? lý xong
        await WaitForTicketCompletionAsync(ticket);
    }

    return orderId;
}
```

### 4. OrderQueueService: Process Queue

```csharp
private async Task ProcessQueueAsync(int productVariantId)
{
    // ?ánh d?u ?ang x? lý
    if (!_processingStatus.TryAdd(productVariantId, true))
    {
        return; // ?ã có ai ?ang x? lý r?i
    }

    try
    {
        var queue = _productQueues.GetOrAdd(productVariantId, _ => new ConcurrentQueue<OrderRequest>());

        // X? lý t?ng request theo th? t? FIFO
        while (queue.TryDequeue(out var request))
        {
            try
            {
                request.Ticket.Status = OrderQueueStatus.Processing;
                request.Ticket.ProcessStartTime = DateTime.UtcNow;

                // Th?c hi?n action (t?o order, tr? stock)
                await request.ProcessAction();

                // Thành công
                request.Ticket.Status = OrderQueueStatus.Completed;
                request.Ticket.ProcessEndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Th?t b?i
                request.Ticket.Status = OrderQueueStatus.Failed;
                request.Ticket.ErrorMessage = ex.Message;
                throw;
            }
        }
    }
    finally
    {
        // ?ánh d?u xong
        _processingStatus.TryRemove(productVariantId, out _);
    }
}
```

## API Endpoints

### 1. ??t Hàng
```
POST /Orders/CreateOrder
Body: { PaymentMethod: 0 }
Response: { success: true, orderId: 123 }
Error: { success: false, message: "S?n ph?m ch? còn 0 s?n ph?m" }
```

### 2. L?y Thông Tin Ticket (Tùy ch?n)
```
GET /api/services/app/Orders/GetQueueTicketInfo?ticketId=abc-123
Response: {
    ticketId: "abc-123",
    position: 2,
    status: 1, // Processing
    estimatedWaitTimeSeconds: 4
}
```

### 3. L?y ?? Dài Queue (Tùy ch?n)
```
GET /api/services/app/Orders/GetProductQueueLength?productVariantId=5
Response: 3 // Có 3 ng??i ?ang ch?
```

## L?i Ích

### 1. Công B?ng (Fairness)
- ? First Come First Served
- ? Không ai b? "nh?y queue"

### 2. Tr?i Nghi?m Ng??i Dùng
- ? Bi?t v? trí trong hàng ch?
- ? ??c tính th?i gian ch?
- ? Thông báo rõ ràng khi th?t b?i

### 3. Tránh Race Condition
- ? Queue ??m b?o th? t? x? lý
- ? M?i ProductVariant có queue riêng
- ? Không x?y ra deadlock

### 4. Scalability
- ? Có th? chuy?n sang Redis Queue:
  ```csharp
  // Future: Distributed Queue
  await _redisQueue.EnqueueAsync($"order:{productId}", request);
  ```

- ? Có th? chuy?n sang RabbitMQ:
  ```csharp
  // Future: Message Queue
  await _rabbitMqBus.PublishAsync(new OrderPlacedEvent { ... });
  ```

## Performance

### Metrics ??c Tính

| S? User ??ng Th?i | X? Lý/Giây | Th?i Gian Ch? Trung Bình |
|-------------------|------------|--------------------------|
| 10 | 5 orders/s | 2 giây |
| 50 | 5 orders/s | 10 giây |
| 100 | 5 orders/s | 20 giây |

**Note**: M?i order m?t ~2 giây (validate + DB operations)

### Bottleneck
- Database I/O (query + update stock)
- Có th? c?i thi?n b?ng caching stock

## C?i Ti?n Trong T??ng Lai

### 1. Real-time Updates v?i SignalR
```csharp
// G?i update v? client
await _hubContext.Clients.User(userId).SendAsync("QueuePositionUpdated", new
{
    position = ticket.Position,
    estimatedWaitTime = ticket.EstimatedWaitTimeSeconds
});
```

### 2. Distributed Queue v?i Redis
```csharp
public class RedisOrderQueueService
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task EnqueueAsync(int productId, OrderRequest request)
    {
        var db = _redis.GetDatabase();
        await db.ListRightPushAsync($"order:queue:{productId}", 
            JsonConvert.SerializeObject(request));
    }
}
```

### 3. Priority Queue
```csharp
// VIP customers có priority cao h?n
public enum QueuePriority
{
    Normal = 0,
    Premium = 1,
    VIP = 2
}
```

### 4. Stock Reservation
```csharp
// ??t ch? t?m th?i khi user vào trang checkout
await _stockReservationService.ReserveAsync(
    productId, 
    quantity, 
    TimeSpan.FromMinutes(10)
);
```

## Testing

### Test Case 1: FIFO Order
```csharp
[Fact]
public async Task CreateOrder_Should_Process_In_FIFO_Order()
{
    // Arrange
    var productId = 1;
    var stock = 2;
    
    // Act
    var task1 = _orderService.CreateOrder(...); // User A
    var task2 = _orderService.CreateOrder(...); // User B
    var task3 = _orderService.CreateOrder(...); // User C
    
    await Task.WhenAll(task1, task2, task3);
    
    // Assert
    Assert.True(task1.IsCompletedSuccessfully); // A thành công
    Assert.True(task2.IsCompletedSuccessfully); // B thành công
    Assert.Throws<UserFriendlyException>(() => task3.Result); // C th?t b?i
}
```

### Test Case 2: Concurrent Orders
```csharp
[Fact]
public async Task CreateOrder_Should_Handle_100_Concurrent_Requests()
{
    // Arrange
    var tasks = Enumerable.Range(1, 100)
        .Select(_ => _orderService.CreateOrder(...))
        .ToArray();
    
    // Act & Assert
    await Task.WhenAll(tasks);
    
    var successCount = tasks.Count(t => t.IsCompletedSuccessfully);
    var stock = await GetCurrentStock(productId);
    
    Assert.Equal(initialStock - successCount, stock);
}
```

## Monitoring

### Dashboard Metrics
- **Queue Length**: S? ng??i ?ang ch?
- **Processing Time**: Th?i gian x? lý trung bình
- **Success Rate**: T? l? ??t hàng thành công
- **Timeout Rate**: T? l? timeout

### Logging
```csharp
_logger.LogInformation(
    "Order queued: TicketId={TicketId}, Position={Position}, ProductId={ProductId}",
    ticket.TicketId, ticket.Position, productId
);
```

## K?t Lu?n

Gi?i pháp Queue FIFO ??m b?o:
- ? **Công b?ng**: Ng??i tr??c ???c x? lý tr??c
- ? **Chính xác**: Không oversell (bán quá s? l??ng)
- ? **Scalable**: Có th? m? r?ng v?i Redis/RabbitMQ
- ? **User-friendly**: Thông báo rõ ràng, bi?t v? trí queue

So v?i SemaphoreSlim, Queue approach ph?c t?p h?n m?t chút nh?ng mang l?i tr?i nghi?m t?t h?n nhi?u cho ng??i dùng và ??m b?o tính công b?ng.
