using Abp.Authorization;
using Abp.Localization;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Notifications
{
	/// <summary>
	/// ??nh ngh?a các lo?i notifications trong h? th?ng
	/// </summary>
	public class SimpleTaskAppNotificationProvider : NotificationProvider
	{
		public override void SetNotifications(INotificationDefinitionContext context)
		{
			// Notification cho ??n hàng m?i
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewOrder",
					displayName: new LocalizableString("NewOrderNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// Notification cho thay ??i tr?ng thái ??n hàng
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderStatusChanged",
					displayName: new LocalizableString("OrderStatusChangedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho ??n hàng ???c duy?t
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderApproved",
					displayName: new LocalizableString("OrderApprovedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho ??n hàng b? t? ch?i
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderRejected",
					displayName: new LocalizableString("OrderRejectedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho ??n hàng hoàn thành
			context.Manager.Add(
				new NotificationDefinition(
					"App.OrderCompleted",
					displayName: new LocalizableString("OrderCompletedNotification", "SimpleTaskApp")
				)
			);

			// Notification cho t?n kho th?p
			context.Manager.Add(
				new NotificationDefinition(
					"App.LowStock",
					displayName: new LocalizableString("LowStockNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// Notification cho s?n ph?m m?i
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewProduct",
					displayName: new LocalizableString("NewProductNotification", "SimpleTaskApp")
				)
			);

			// Notification cho bình lu?n s?n ph?m m?i (Admin only)
			context.Manager.Add(
				new NotificationDefinition(
					"App.NewProductComment",
					displayName: new LocalizableString("NewProductCommentNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
				)
			);

			// Notification khi có ng??i reply comment (User & Admin)
			context.Manager.Add(
				new NotificationDefinition(
					"App.CommentReply",
					displayName: new LocalizableString("CommentReplyNotification", "SimpleTaskApp")
				)
			);

			// ? NEW: Notification báo cáo doanh thu hàng ngày (Admin only)
			context.Manager.Add(
				new NotificationDefinition(
					"App.DailyRevenueReport",
					displayName: new LocalizableString("DailyRevenueReportNotification", "SimpleTaskApp"),
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);

			// ? NEW: Notification c?nh báo t?n kho th?p hàng ngày (Admin only)
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
