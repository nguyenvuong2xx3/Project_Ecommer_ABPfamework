using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Orders.OrderStatusNotificationJob;

namespace Acme.SimpleTaskApp.Orders
{
	public class OrderStatusNotificationJob : AsyncBackgroundJob<OrderStatusNotificationJobArgs>, ITransientDependency
	{
		private readonly INotificationPublisher _notificationPublisher;

		public OrderStatusNotificationJob(INotificationPublisher notificationPublisher)
		{
			_notificationPublisher = notificationPublisher;
		}

		public override async Task ExecuteAsync(OrderStatusNotificationJobArgs args)
		{
			var notificationData = new NotificationData();
			notificationData["Message"] = args.Message;
			notificationData["Title"] = args.Title;
			notificationData["OrderCode"] = args.OrderCode;
			notificationData["OrderId"] = args.OrderId.ToString();
			notificationData["Type"] = args.NotificationType;
			notificationData["Status"] = args.Status.ToString();

			await _notificationPublisher.PublishAsync(
					args.NotificationName,
					notificationData,
					severity: args.Severity,
					userIds: new[] { new UserIdentifier(args.TenantId, args.UserId) }
			);
		}

		public class OrderStatusNotificationJobArgs
		{
			public int TenantId { get; set; }
			public long UserId { get; set; }
			public string OrderCode { get; set; }
			public int OrderId { get; set; }
			public string NotificationName { get; set; }
			public string Title { get; set; }
			public string Message { get; set; }
			public string NotificationType { get; set; }
			public int Status { get; set; }
			public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
		}
	}
}
