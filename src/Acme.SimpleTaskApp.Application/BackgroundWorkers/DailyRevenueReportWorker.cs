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
using Acme.SimpleTaskApp.Orders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.BackgroundWorkers
{
	/// Gửi báo cáo doanh thu cho admin
	public class DailyRevenueReportWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
	{
		private readonly IRepository<Order, int> _orderRepository;
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;
		private readonly IBackgroundJobManager _backgroundJobManager;
		private readonly ISettingManager _settingManager;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public const string NotificationName = "App.DailyRevenueReport";

		private const int TargetHour = 22;
		private const int TargetMinute = 0;

		// Kiểm tra mỗi phút
		private const int CheckIntervalMinutes = 1;

		// Cài đặt thử lại nếu thất bại
		private const int MaxRetryCount = 3;
		private const int RetryDelayMinutes = 5;
		private int _currentRetryCount = 0;
		private DateTime? _nextRetryTime = null;

		public DailyRevenueReportWorker(
			AbpAsyncTimer timer,
			IRepository<Order, int> orderRepository,
			INotificationPublisher notificationPublisher,
			UserManager userManager,
			IBackgroundJobManager backgroundJobManager,
			ISettingManager settingManager,
			IWebHostEnvironment webHostEnvironment)
			: base(timer)
		{
			_orderRepository = orderRepository;
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
				AppBackgroundWorkerSettings.DailyRevenueReport_LastExecutionDate);

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
				await ExecuteDailyReportAsync(reportDate);

				// Thành công - lưu vào database
				await _settingManager.ChangeSettingForApplicationAsync(
					AppBackgroundWorkerSettings.DailyRevenueReport_LastExecutionDate,
					reportDate.ToString("yyyy-MM-dd"));

				// Reset retry state
				_currentRetryCount = 0;
				_nextRetryTime = null;

				Logger.Info($"Daily revenue report executed successfully for {reportDate:dd/MM/yyyy}. State saved to database.");
			}
			catch (Exception ex)
			{
				_currentRetryCount++;
				Logger.Error($"Error executing daily revenue report (Attempt {_currentRetryCount}/{MaxRetryCount})", ex);

				if (_currentRetryCount < MaxRetryCount)
				{
					// Đặt lịch retry
					_nextRetryTime = DateTime.Now.AddMinutes(RetryDelayMinutes);
					Logger.Warn($"Will retry at {_nextRetryTime:HH:mm:ss}");
				}
				else
				{
					// Đã hết số lần retry
					Logger.Error($"Daily revenue report failed after {MaxRetryCount} attempts. Will try again tomorrow.");
					_currentRetryCount = 0;
					_nextRetryTime = null;
				}
			}
		}

		private async Task ExecuteDailyReportAsync(DateTime reportDate)
		{
			var tomorrow = reportDate.AddDays(1);

			// Lấy tất cả đơn hàng trong ngày báo cáo
			var todayOrders = await _orderRepository.GetAll()
				.Where(o => o.CreationTime >= reportDate && o.CreationTime < tomorrow)
				.ToListAsync();

			// Tính toán thống kê
			var totalOrders = todayOrders.Count;
			var completedOrders = todayOrders.Count(o => o.Status == 3); // Status 3 = Hoàn thành
			var pendingOrders = todayOrders.Count(o => o.Status == 0); // Status 0 = Chờ xác nhận
			var totalRevenue = todayOrders.Where(o => o.Status == 3).Sum(o => o.TotalPrice ?? 0);
			var cancelledOrders = todayOrders.Count(o => o.Status == 4 || o.Status == 5); // 4 = Admin hủy, 5 = User hủy

			// Lấy tất cả admin users
			var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

			if (adminUsers.Count == 0)
			{
				Logger.Warn("No admin users found to send daily revenue report.");
				return;
			}

			// 1. Gửi Notification
			await SendNotificationAsync(reportDate, totalOrders, completedOrders, pendingOrders, cancelledOrders, totalRevenue, adminUsers);

			// 2. Gửi Email
			await SendEmailAsync(reportDate, totalOrders, completedOrders, pendingOrders, cancelledOrders, totalRevenue, adminUsers);

			Logger.Info($"Daily revenue report sent successfully. Date: {reportDate:dd/MM/yyyy}, Orders: {totalOrders}, Revenue: {totalRevenue:N0}đ");
		}

		private async Task SendNotificationAsync(
			DateTime today, 
			int totalOrders, 
			int completedOrders, 
			int pendingOrders, 
			int cancelledOrders, 
			decimal totalRevenue,
			System.Collections.Generic.IList<User> adminUsers)
		{
			var notificationData = new NotificationData();
			notificationData["Date"] = today.ToString("dd/MM/yyyy");
			notificationData["TotalOrders"] = totalOrders;
			notificationData["CompletedOrders"] = completedOrders;
			notificationData["PendingOrders"] = pendingOrders;
			notificationData["CancelledOrders"] = cancelledOrders;
			notificationData["TotalRevenue"] = totalRevenue;
			notificationData["FormattedRevenue"] = totalRevenue.ToString("N0") + "đ";
			notificationData["Type"] = "DailyRevenueReport";
			notificationData["Message"] = $"Báo cáo doanh thu ngày {today:dd/MM/yyyy}: " +
				$"{totalOrders} đơn hàng, " +
				$"{completedOrders} hoàn thành, " +
				$"Doanh thu: {totalRevenue:N0}đ";

			var userIds = adminUsers.Select(u => u.ToUserIdentifier()).ToArray();

			await _notificationPublisher.PublishAsync(
				NotificationName,
				notificationData,
				severity: NotificationSeverity.Info,
				userIds: userIds
			);
		}

		private async Task SendEmailAsync(
			DateTime today,
			int totalOrders,
			int completedOrders,
			int pendingOrders,
			int cancelledOrders,
			decimal totalRevenue,
			System.Collections.Generic.IList<User> adminUsers)
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
				var templatePath = Path.Combine(webRootPath, "assets", "EmailTemplates", "EmailDailyRevenueReport.html");

				if (!File.Exists(templatePath))
				{
					Logger.Warn($"Email template not found: {templatePath}");
					return;
				}

				var htmlBody = await File.ReadAllTextAsync(templatePath);

				htmlBody = htmlBody
					.Replace("{{StoreName}}", storeName)
					.Replace("{{LogoUrl}}", logoUrl ?? "")
					.Replace("{{ReportDate}}", today.ToString("dd/MM/yyyy"))
					.Replace("{{TotalRevenue}}", totalRevenue.ToString("N0") + "đ")
					.Replace("{{TotalOrders}}", totalOrders.ToString())
					.Replace("{{CompletedOrders}}", completedOrders.ToString())
					.Replace("{{PendingOrders}}", pendingOrders.ToString())
					.Replace("{{CancelledOrders}}", cancelledOrders.ToString())
					.Replace("{{DashboardUrl}}", "/Home")
					.Replace("{{SendTime}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

				var adminEmails = adminUsers
					.Where(u => !string.IsNullOrWhiteSpace(u.EmailAddress))
					.Select(u => u.EmailAddress)
					.ToList();

				if (adminEmails.Count == 0)
				{
					Logger.Warn("No admin emails found to send daily revenue report.");
					return;
				}

				await _backgroundJobManager.EnqueueAsync<SendEmailToAdminsBackgroundJob, SendEmailToAdminsJobArgs>(
					new SendEmailToAdminsJobArgs
					{
						AdminEmails = adminEmails,
						Subject = $"[{storeName}] Báo cáo doanh thu ngày {today:dd/MM/yyyy}",
						Body = htmlBody
					});

				Logger.Info($"Daily revenue report email queued for {adminEmails.Count} admin(s).");
			}
			catch (Exception ex)
			{
				Logger.Error("Error sending daily revenue report email", ex);
				throw; // Re-throw để trigger retry
			}
		}
	}
}
