using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization.Users;
using System.Linq;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Orders.OrderNotificationJob;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrderNotificationJob : AsyncBackgroundJob<OrderNotificationJobArgs>, ITransientDependency
	{
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;

		// Định nghĩa hằng số tên thông báo để dùng chung
		public const string NotificationName = "App.NewOrder";

		public OrderNotificationJob(
				INotificationPublisher notificationPublisher,
				UserManager userManager)
		{
			_notificationPublisher = notificationPublisher;
			_userManager = userManager;
		}

		public override async Task ExecuteAsync(OrderNotificationJobArgs args)
		{
			var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

			// Tạo data
			var notificationData = new NotificationData();
			// Lưu message vào properties để JS lấy ra hiển thị
			notificationData["Message"] = $"Có đơn hàng mới #{args.Code} được tạo";
			notificationData["Code"] = args.Code;
			notificationData["Type"] = "NewOrder";

			if (adminUsers.Count > 0)
			{
				var userIds = adminUsers.Select(u => u.ToUserIdentifier()).ToArray();

				await _notificationPublisher.PublishAsync(
						NotificationName, // Sử dụng tên chuẩn "App.NewOrder"
						notificationData,
						severity: NotificationSeverity.Info,
						userIds: userIds
				);
			}
		}

		public class OrderNotificationJobArgs
		{
			public string Code { get; set; }
		}
	}
	
}