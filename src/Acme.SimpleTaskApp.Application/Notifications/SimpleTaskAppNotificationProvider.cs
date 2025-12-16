using Abp.Authorization;
using Abp.Localization;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Notifications
{
	/// Định nghĩa các loại notifications trong hệ thống
	public class SimpleTaskAppNotificationProvider : NotificationProvider
	{
		public override void SetNotifications(INotificationDefinitionContext context)
		{
			// Notification cho đơn hàng mới
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewOrder",
					displayName: new LocalizableString("NewOrderNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// Notification cho thay đổi trạng thái đơn hàng
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderStatusChanged",
					displayName: new LocalizableString("OrderStatusChangedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho đơn hàng được duyệt
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderApproved",
					displayName: new LocalizableString("OrderApprovedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho đơn hàng bị từ chối
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderRejected",
					displayName: new LocalizableString("OrderRejectedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho đơn hàng hoàn thành
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderCompleted",
					displayName: new LocalizableString("OrderCompletedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho tồn kho thấp
			context.Manager.Add(
				new NotificationDefinition(
					"App.LowStock",
					displayName: new LocalizableString("LowStockNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// Notification cho sản phẩm mới
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewProduct",
					displayName: new LocalizableString("NewProductNotification", "SimpleTaskApp")
				)
			);

			// Notification cho bình luận sản phẩm mới (Admin only)
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewProductComment",
					displayName: new LocalizableString("NewProductCommentNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
				)
			);

			// Notification khi có người reply comment (User & Admin)
			context.Manager.Add(
				new NotificationDefinition(
					"App.CommentReply",
					displayName: new LocalizableString("CommentReplyNotification", "SimpleTaskApp")
				)
			);

			// Notification báo cáo doanh thu hàng ngày (Admin only)
			context.Manager.Add(
				new NotificationDefinition(
					"App.DailyRevenueReport",
					displayName: new LocalizableString("DailyRevenueReportNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// Notification cảnh báo tồn kho thấp hàng ngày (Admin only)
			context.Manager.Add(
				new NotificationDefinition(
					"App.LowStockAlert",
					displayName: new LocalizableString("LowStockAlertNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);
		}
	}
}
