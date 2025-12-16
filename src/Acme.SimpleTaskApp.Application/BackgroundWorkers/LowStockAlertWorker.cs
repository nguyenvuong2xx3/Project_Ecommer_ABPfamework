using Abp;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Notifications;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Configuration;
using Acme.SimpleTaskApp.Email;
using Acme.SimpleTaskApp.Email.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.BackgroundWorkers
{
	/// <summary>
	/// Background worker chạy vào 22h mỗi ngày để cảnh báo sản phẩm sắp hết hàng (số lượng < 3).
	/// - Lưu trạng thái vào database (AbpSettings) để persist qua restart
	/// - Tự động chạy bù nếu server down lúc 22h
	/// - Có cơ chế retry nếu job fail
	/// </summary>
	public class LowStockAlertWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
	{
		private readonly IRepository<ProductVariant, int> _productVariantRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;
		private readonly IBackgroundJobManager _backgroundJobManager;
		private readonly ISettingManager _settingManager;
		private readonly IWebHostEnvironment _webHostEnvironment;

		// Notification name constant
		public const string NotificationName = "App.LowStockAlert";

		// Ngưỡng cảnh báo tồn kho thấp
		public const int LowStockThreshold = 3;

		// Cài thời gian target: 22h (10 PM)
		private const int TargetHour = 11;
		private const int TargetMinute = 05;

		// Kiểm tra mỗi phút
		private const int CheckIntervalMinutes = 1;

		// Retry configuration
		private const int MaxRetryCount = 3;
		private const int RetryDelayMinutes = 5;
		private int _currentRetryCount = 0;
		private DateTime? _nextRetryTime = null;

		public LowStockAlertWorker(
			AbpAsyncTimer timer,
			IRepository<ProductVariant, int> productVariantRepository,
			IRepository<Product, int> productRepository,
			INotificationPublisher notificationPublisher,
			UserManager userManager,
			IBackgroundJobManager backgroundJobManager,
			ISettingManager settingManager,
			IWebHostEnvironment webHostEnvironment)
			: base(timer)
		{
			_productVariantRepository = productVariantRepository;
			_productRepository = productRepository;
			_notificationPublisher = notificationPublisher;
			_userManager = userManager;
			_backgroundJobManager = backgroundJobManager;
			_settingManager = settingManager;
			_webHostEnvironment = webHostEnvironment;

			// Kiểm tra mỗi phút
			Timer.Period = CheckIntervalMinutes * 60 * 1000; // milliseconds
		}

		[UnitOfWork]
		protected override async Task DoWorkAsync()
		{
			var now = DateTime.Now;
			var today = DateTime.Today;

			// Lấy ngày chạy cuối cùng từ database
			var lastExecutionDateStr = await _settingManager.GetSettingValueAsync(
				AppBackgroundWorkerSettings.LowStockAlert_LastExecutionDate);

			DateTime? lastExecutionDate = null;
			if (DateTime.TryParse(lastExecutionDateStr, out var parsedDate))
			{
				lastExecutionDate = parsedDate;
			}

			// Kiểm tra xem hôm nay đã chạy thành công chưa
			bool alreadyExecutedToday = lastExecutionDate?.Date == today;

			if (alreadyExecutedToday)
			{
				// Reset retry count nếu đã chạy thành công
				_currentRetryCount = 0;
				_nextRetryTime = null;
				return;
			}

			// Kiểm tra nếu đang trong chế độ retry
			if (_nextRetryTime.HasValue && now < _nextRetryTime.Value)
			{
				return; // Chưa đến thời gian retry
			}

			// Điều kiện để chạy:
			// 1. Đúng giờ target (22:00)
			// 2. HOẶC đã qua giờ target và chưa chạy hôm nay (chạy bù)
			// 3. HOẶC đang retry sau khi fail
			bool isTargetTime = now.Hour == TargetHour && now.Minute == TargetMinute;
			bool isMissedAndNeedCatchUp = now.Hour >= TargetHour && !alreadyExecutedToday && lastExecutionDate?.Date != today;
			bool isRetrying = _nextRetryTime.HasValue && now >= _nextRetryTime.Value;

			if (isTargetTime || isMissedAndNeedCatchUp || isRetrying)
			{
				await ExecuteWithRetryAsync(today);
			}
		}

		private async Task ExecuteWithRetryAsync(DateTime reportDate)
		{
			try
			{
				await ExecuteLowStockAlertAsync();

				// Thành công - lưu vào database
				await _settingManager.ChangeSettingForApplicationAsync(AppBackgroundWorkerSettings.LowStockAlert_LastExecutionDate,reportDate.ToString("yyyy-MM-dd"));

				// Reset retry state
				_currentRetryCount = 0;
				_nextRetryTime = null;

				Logger.Info($"Low stock alert executed successfully for {reportDate:dd/MM/yyyy}. State saved to database.");
			}
			catch (Exception ex)
			{
				_currentRetryCount++;
				Logger.Error($"Error executing low stock alert (Attempt {_currentRetryCount}/{MaxRetryCount})", ex);

				if (_currentRetryCount < MaxRetryCount)
				{
					// Đặt lịch retry
					_nextRetryTime = DateTime.Now.AddMinutes(RetryDelayMinutes);
					Logger.Warn($"Will retry at {_nextRetryTime:HH:mm:ss}");
				}
				else
				{
					// Đã hết số lần retry
					Logger.Error($"Low stock alert failed after {MaxRetryCount} attempts. Will try again tomorrow.");
					_currentRetryCount = 0;
					_nextRetryTime = null;
				}
			}
		}

		private async Task ExecuteLowStockAlertAsync()
		{
			// Lấy tất cả biến thể sản phẩm có số lượng tồn kho < threshold
			var lowStockVariants = await _productVariantRepository.GetAll()
				.Where(v => v.StockQuantity < LowStockThreshold)
				.ToListAsync();

			if (!lowStockVariants.Any())
			{
				Logger.Info("No low stock products found.");
				return;
			}

			// Lấy thông tin sản phẩm
			var productIds = lowStockVariants.Select(v => v.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll().Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);

			// Tạo danh sách sản phẩm sắp hết hàng
			var lowStockItems = new List<LowStockItem>();

			foreach (var variant in lowStockVariants)
			{
				var productName = products.ContainsKey(variant.ProductId) 
					? products[variant.ProductId] 
					: "Unknown Product";

				lowStockItems.Add(new LowStockItem
				{
					ProductId = variant.ProductId,
					ProductName = productName,
					VariantId = variant.Id,
					VariantInfo = $"{variant.Ram} - {variant.Storage} - {variant.Color}",
					StockQuantity = variant.StockQuantity
				});
			}

			// Sắp xếp theo số lượng tồn kho tăng dần
			lowStockItems = lowStockItems.OrderBy(x => x.StockQuantity).ToList();/**/

			// Lấy tất cả admin users
			var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

			if (adminUsers.Count == 0)/**/
			{
				Logger.Warn("No admin users found to send low stock alert.");
				return;
			}

			// 1. Gửi Notification
			await SendNotificationAsync(lowStockItems, adminUsers);

			// 2. Gửi Email
			await SendEmailAsync(lowStockItems, adminUsers);

			Logger.Info($"Low stock alert sent successfully. Total low stock items: {lowStockItems.Count}");
		}

		private async Task SendNotificationAsync(List<LowStockItem> lowStockItems, System.Collections.Generic.IList<User> adminUsers)
		{
			var notificationData = new NotificationData();
			notificationData["TotalLowStockItems"] = lowStockItems.Count;
			notificationData["Threshold"] = LowStockThreshold;
			notificationData["Type"] = "LowStockAlert";
			notificationData["Date"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

			// Tạo message chi tiết (giới hạn 5 sản phẩm đầu tiên)
			var topItems = lowStockItems.Take(5).ToList();
			var itemsMessage = string.Join("\n", topItems.Select(x => 
				$"• {x.ProductName} ({x.VariantInfo}): còn {x.StockQuantity} sản phẩm"));

			var remainingCount = lowStockItems.Count - 5;
			if (remainingCount > 0)
			{
				itemsMessage += $"\n... và {remainingCount} sản phẩm khác";
			}

			notificationData["ItemsDetail"] = itemsMessage;
			notificationData["Message"] = $"⚠️ Cảnh báo tồn kho thấp: {lowStockItems.Count} biến thể sản phẩm có số lượng < {LowStockThreshold}. Chi tiết đã được gửi đến email của bạn";

			// Lưu danh sách chi tiết (dạng JSON string)
			notificationData["LowStockItems"] = System.Text.Json.JsonSerializer.Serialize(
				lowStockItems.Select(x => new
				{
					x.ProductId,
					x.ProductName,
					x.VariantId,
					x.VariantInfo,
					x.StockQuantity
				})
			);

			var userIds = adminUsers.Select(u => u.ToUserIdentifier()).ToArray();

			await _notificationPublisher.PublishAsync(
				NotificationName,
				notificationData,
				severity: NotificationSeverity.Warn,
				userIds: userIds
			);
		}

		private async Task SendEmailAsync(List<LowStockItem> lowStockItems, System.Collections.Generic.IList<User> adminUsers)
		{
			try
			{
				var storeName = await _settingManager.GetSettingValueAsync(AppNameStore.NameStore);
				var logoUrl = await _settingManager.GetSettingValueAsync(AppNameStore.UrlLogo);

				if (string.IsNullOrWhiteSpace(storeName))
				{
					storeName = "Cửa hàng của bạn";
				}

				var webRootPath = _webHostEnvironment.WebRootPath;
				var templatePath = Path.Combine(webRootPath, "assets", "EmailTemplates", "EmailLowStockAlert.html");

				if (!File.Exists(templatePath))
				{
					Logger.Warn($"Email template not found: {templatePath}");
					return;
				}

				var htmlBody = await File.ReadAllTextAsync(templatePath);

				// Tạo HTML cho danh sách sản phẩm (tối đa 10 sản phẩm)
				var productRowsHtml = new StringBuilder();
				var displayItems = lowStockItems.Take(10).ToList();
				var index = 1;

				foreach (var item in displayItems)
				{
					var stockClass = item.StockQuantity == 0 ? "stock-critical" 
						: item.StockQuantity == 1 ? "stock-low" 
						: "stock-warning";

					productRowsHtml.AppendLine($@"
						<tr>
							<td>{index++}</td>
							<td>
								<div class=""product-name"">{item.ProductName}</div>
							</td>
							<td>
								<div class=""variant-info"">{item.VariantInfo}</div>
							</td>
							<td class=""{stockClass}"">{item.StockQuantity}</td>
						</tr>");
				}

				// Phần hiển thị thêm nếu có nhiều hơn 10 sản phẩm
				var moreItemsSection = "";
				if (lowStockItems.Count > 10)
				{
					moreItemsSection = $@"<div class=""more-items"">... và {lowStockItems.Count - 10} sản phẩm khác</div>";
				}

				htmlBody = htmlBody
					.Replace("{{StoreName}}", storeName)
					.Replace("{{LogoUrl}}", logoUrl ?? "")
					.Replace("{{ReportDate}}", DateTime.Now.ToString("dd/MM/yyyy"))
					.Replace("{{TotalLowStockItems}}", lowStockItems.Count.ToString())
					.Replace("{{Threshold}}", LowStockThreshold.ToString())
					.Replace("{{ProductRows}}", productRowsHtml.ToString())
					.Replace("{{MoreItemsSection}}", moreItemsSection)
					.Replace("{{ProductsUrl}}", "/ProductVariants")
					.Replace("{{SendTime}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

				var adminEmails = adminUsers
					.Where(u => !string.IsNullOrWhiteSpace(u.EmailAddress))
					.Select(u => u.EmailAddress)
					.ToList();

				if (adminEmails.Count == 0)
				{
					Logger.Warn("No admin emails found to send low stock alert.");
					return;
				}

				await _backgroundJobManager.EnqueueAsync<SendEmailToAdminsBackgroundJob, SendEmailToAdminsJobArgs>(
					new SendEmailToAdminsJobArgs
					{
						AdminEmails = adminEmails,
						Subject = $"[{storeName}] Cảnh báo: {lowStockItems.Count} sản phẩm sắp hết hàng.",
						Body = htmlBody
					});

				Logger.Info($"Low stock alert email queued for {adminEmails.Count} admin(s).");
			}
			catch (Exception ex)
			{
				Logger.Error("Error sending low stock alert email", ex);
				throw; // Re-throw để trigger retry
			}
		}

		private class LowStockItem
		{
			public int ProductId { get; set; }
			public string ProductName { get; set; }
			public int VariantId { get; set; }
			public string VariantInfo { get; set; }
			public int StockQuantity { get; set; }
		}
	}
}
