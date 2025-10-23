using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Dependency;

namespace Acme.SimpleTaskApp.Orders
{
	/// <summary>
	/// Service quản lý Queue FIFO cho việc đặt hàng
	/// Đảm bảo người vào trước được xử lý trước (First In First Out)
	/// </summary>
	public class OrderQueueService : ISingletonDependency
	{
		// Dictionary lưu Queue cho mỗi ProductVariant
		// Key: ProductVariantId, Value: Queue các order request
		private readonly ConcurrentDictionary<int, ConcurrentQueue<OrderRequest>> _productQueues;
		
		// Dictionary lưu trạng thái xử lý cho mỗi ProductVariant
		// Key: ProductVariantId, Value: true nếu đang xử lý
		private readonly ConcurrentDictionary<int, bool> _processingStatus;
		
		// Lock object cho mỗi ProductVariant
		private readonly ConcurrentDictionary<int, SemaphoreSlim> _productLocks;

		public OrderQueueService()
		{
			_productQueues = new ConcurrentDictionary<int, ConcurrentQueue<OrderRequest>>();
			_processingStatus = new ConcurrentDictionary<int, bool>();
			_productLocks = new ConcurrentDictionary<int, SemaphoreSlim>();
		}

		/// <summary>
		/// Thêm order request vào queue
		/// </summary>
		public async Task<OrderQueueTicket> EnqueueOrderRequest(int productVariantId, int quantity, Func<Task> processAction)
		{
			// Tạo ticket cho request này
			var ticket = new OrderQueueTicket
			{
				TicketId = Guid.NewGuid().ToString(),
				ProductVariantId = productVariantId,
				Quantity = quantity,
				EnqueueTime = DateTime.UtcNow,
				Status = OrderQueueStatus.Waiting
			};

			var request = new OrderRequest
			{
				Ticket = ticket,
				ProcessAction = processAction
			};

			// Lấy hoặc tạo queue cho ProductVariant
			var queue = _productQueues.GetOrAdd(productVariantId, _ => new ConcurrentQueue<OrderRequest>());
			
			// Thêm vào queue
			queue.Enqueue(request);
			ticket.Position = queue.Count;

			// Bắt đầu xử lý queue nếu chưa có ai xử lý
			_ = Task.Run(() => ProcessQueueAsync(productVariantId));

			return ticket;
		}

		/// <summary>
		/// Xử lý queue cho một ProductVariant
		/// </summary>
		private async Task ProcessQueueAsync(int productVariantId)
		{
			// Kiểm tra xem có ai đang xử lý queue này không
			if (!_processingStatus.TryAdd(productVariantId, true))
			{
				// Đã có ai đang xử lý rồi
				return;
			}

			try
			{
				var queue = _productQueues.GetOrAdd(productVariantId, _ => new ConcurrentQueue<OrderRequest>());

				while (queue.TryDequeue(out var request))
				{
					try
					{
						// Cập nhật trạng thái
						request.Ticket.Status = OrderQueueStatus.Processing;
						request.Ticket.ProcessStartTime = DateTime.UtcNow;

						// Thực hiện action (tạo order, trừ stock, v.v.)
						await request.ProcessAction();

						// Thành công
						request.Ticket.Status = OrderQueueStatus.Completed;
						request.Ticket.ProcessEndTime = DateTime.UtcNow;
					}
					catch (Exception ex)
					{
						// Thất bại
						request.Ticket.Status = OrderQueueStatus.Failed;
						request.Ticket.ErrorMessage = ex.Message;
						request.Ticket.ProcessEndTime = DateTime.UtcNow;
						
						// Nếu muốn, có thể throw lại exception để caller biết
						throw;
					}
				}
			}
			finally
			{
				// Đánh dấu là đã xử lý xong
				_processingStatus.TryRemove(productVariantId, out _);
			}
		}

		/// <summary>
		/// Lấy vị trí hiện tại trong queue
		/// </summary>
		public int GetQueuePosition(int productVariantId, string ticketId)
		{
			if (!_productQueues.TryGetValue(productVariantId, out var queue))
			{
				return 0;
			}

			var requests = queue.ToArray();
			for (int i = 0; i < requests.Length; i++)
			{
				if (requests[i].Ticket.TicketId == ticketId)
				{
					return i + 1; // Position is 1-based
				}
			}

			return 0;
		}

		/// <summary>
		/// Lấy thông tin ticket
		/// </summary>
		public OrderQueueTicket GetTicketInfo(string ticketId)
		{
			foreach (var queue in _productQueues.Values)
			{
				var request = queue.FirstOrDefault(r => r.Ticket.TicketId == ticketId);
				if (request != null)
				{
					return request.Ticket;
				}
			}
			return null;
		}

		/// <summary>
		/// Lấy số lượng người đang chờ trong queue
		/// </summary>
		public int GetQueueLength(int productVariantId)
		{
			if (!_productQueues.TryGetValue(productVariantId, out var queue))
			{
				return 0;
			}
			return queue.Count;
		}
	}

	/// <summary>
	/// Request trong queue
	/// </summary>
	public class OrderRequest
	{
		public OrderQueueTicket Ticket { get; set; }
		public Func<Task> ProcessAction { get; set; }
	}

	/// <summary>
	/// Ticket theo dõi trạng thái của order request
	/// </summary>
	public class OrderQueueTicket
	{
		public string TicketId { get; set; }
		public int ProductVariantId { get; set; }
		public int Quantity { get; set; }
		public int Position { get; set; }
		public DateTime EnqueueTime { get; set; }
		public DateTime? ProcessStartTime { get; set; }
		public DateTime? ProcessEndTime { get; set; }
		public OrderQueueStatus Status { get; set; }
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Thời gian chờ ước tính (giây)
		/// </summary>
		public int EstimatedWaitTimeSeconds => Position * 2; // Giả sử mỗi order mất 2 giây
	}

	/// <summary>
	/// Trạng thái của order trong queue
	/// </summary>
	public enum OrderQueueStatus
	{
		Waiting = 0,      // Đang chờ
		Processing = 1,   // Đang xử lý
		Completed = 2,    // Hoàn thành
		Failed = 3        // Thất bại
	}
}
