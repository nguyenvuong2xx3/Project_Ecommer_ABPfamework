using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Orders.OrderNotificationJob;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrderNotificationJob : AsyncBackgroundJob<OrderNotificationJobArgs>, ITransientDependency
	{
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;

		public OrderNotificationJob(
				INotificationPublisher notificationPublisher,
				UserManager userManager)
		{
			_notificationPublisher = notificationPublisher;
			_userManager = userManager;
		}

		public override async Task ExecuteAsync(OrderNotificationJobArgs args)
		{
			// 1. Lấy tất cả user có role Admin
			var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

			// 2. Tạo nội dung thông báo trực tiếp
			var notificationData = new NotificationData();
			notificationData["Message"] = $"Có đơn hàng mới #{args.Code} được tạo";
			notificationData["Code"] = args.Code;
			notificationData["Type"] = "NewOrder";

			// 3. Gửi thông báo đến từng admin
			foreach (var admin in adminUsers)
			{
				await _notificationPublisher.PublishAsync(
						"Đơn hàng mới", // Tên loại thông báo
						notificationData,
						severity: NotificationSeverity.Info,
						userIds: new[] { admin.ToUserIdentifier() } // Gửi đến admin cụ thể
				);
			}
		}

		public class OrderNotificationJobArgs
		{
			public string Code { get; set; }
		}
	}
}