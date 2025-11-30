namespace Acme.SimpleTaskApp.Authorization
{
	public static class PermissionNames
	{
		// ===== DASHBOARD =====
		public const string Pages_Dashboard = "Pages.Dashboard";

		// ===== ADMINISTRATION =====
		public const string Pages_Tenants = "Pages.Tenants";

		public const string Pages_Users = "Pages.Users";
		public const string Pages_Users_Create = "Pages.Users.Create";
		public const string Pages_Users_Edit = "Pages.Users.Edit";
		public const string Pages_Users_Delete = "Pages.Users.Delete";
		public const string Pages_Users_Activation = "Pages.Users.Activation";
		public const string Pages_Users_Impersonation = "Pages.Users.Impersonation";
		public const string Pages_Users_ChangePermissions = "Pages.Users.ChangePermissions";

		public const string Pages_Roles = "Pages.Roles";
		public const string Pages_Roles_Create = "Pages.Roles.Create";
		public const string Pages_Roles_Edit = "Pages.Roles.Edit";
		public const string Pages_Roles_Delete = "Pages.Roles.Delete";

		public const string Pages_Settings = "Pages.Settings";
		public const string Pages_Settings_Edit = "Pages.Settings.Edit";

		// ===== CONTENT MANAGEMENT =====
		public const string Pages_Banners = "Pages.Banners";
		public const string Pages_Banners_Create = "Pages.Banners.Create";
		public const string Pages_Banners_Edit = "Pages.Banners.Edit";
		public const string Pages_Banners_Delete = "Pages.Banners.Delete";

		public const string Pages_Sales = "Pages.Sales";
		public const string Pages_Sales_Create = "Pages.Sales.Create";
		public const string Pages_Sales_Edit = "Pages.Sales.Edit";
		public const string Pages_Sales_Delete = "Pages.Sales.Delete";
		public const string Pages_Sales_ToggleActive = "Pages.Sales.ToggleActive";

		// ===== PRODUCT MANAGEMENT =====
		public const string Pages_categories = "Pages.category";
		public const string Pages_category_create = "Pages.category.create";
		public const string Pages_category_update = "Pages.category.update";
		public const string Pages_category_delete = "Pages.category.delete";

		public const string Pages_products = "Pages.product";
		public const string Pages_products_create = "Pages.product.create";
		public const string Pages_products_update = "Pages.product.update";
		public const string Pages_products_delete = "Pages.product.delete";
		public const string Pages_products_search = "Pages.product.search";
		public const string Pages_products_view = "Pages.product.view";
		public const string Pages_products_import = "Pages.product.import";
		public const string Pages_products_export = "Pages.product.export";

		public const string Pages_productVariants = "Pages.ProductVariants";
		public const string Pages_productVariants_create = "Pages.ProductVariants.Create";
		public const string Pages_productVariants_update = "Pages.ProductVariants.Update";
		public const string Pages_productVariants_delete = "Pages.ProductVariants.Delete";
		public const string Pages_productVariants_view = "Pages.ProductVariants.View";

		// ===== SALES MANAGEMENT =====
		public const string Pages_homeCustomer = "Pages.homeCustomer";

		public const string Pages_orders = "Pages.orders";
		public const string Pages_orders_create = "Pages.orders.create";
		public const string Pages_orders_view = "Pages.orders.view";
		public const string Pages_orders_edit = "Pages.orders.edit";
		public const string Pages_orders_delete = "Pages.orders.delete";
		public const string Pages_orders_confirm = "Pages.orders.confirm";
		public const string Pages_orders_cancel = "Pages.orders.cancel";
		public const string Pages_orders_ship = "Pages.orders.ship";
		public const string Pages_orders_complete = "Pages.orders.complete";

		public const string Pages_carts = "Pages.Carts";
		public const string Pages_carts_addItem = "Pages.Carts.AddItem";
		public const string Pages_carts_removeItem = "Pages.Carts.RemoveItem";
		public const string Pages_carts_view = "Pages.Carts.View";
		public const string Pages_carts_clear = "Pages.Carts.Clear";

		// ===== NOTIFICATIONS =====
		public const string Pages_notifications = "Pages.Notifications";
		public const string Pages_notifications_view = "Pages.Notifications.View";
		public const string Pages_notifications_setAsRead = "Pages.Notifications.SetAsRead";
		public const string Pages_notifications_delete = "Pages.Notifications.Delete";
		public const string Pages_notifications_settings = "Pages.Notifications.Settings";
	}
}
