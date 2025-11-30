using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Acme.SimpleTaskApp.Authorization
{
	public class SimpleTaskAppAuthorizationProvider : AuthorizationProvider
	{
		public override void SetPermissions(IPermissionDefinitionContext context)
		{
			// ===== DASHBOARD =====
			context.CreatePermission(PermissionNames.Pages_Dashboard, L("Dashboard"));

			// ===== ADMINISTRATION =====
			context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

			// Users
			var users = context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Create, L("CreateNewUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Edit, L("EditUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Delete, L("DeleteUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Impersonation, L("LoginForUsers"));
			users.CreateChildPermission(PermissionNames.Pages_Users_ChangePermissions, L("ChangePermissions"));

			// Roles
			var roles = context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Create, L("CreateNewRole"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Edit, L("EditRole"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Delete, L("DeleteRole"));

			// Settings
			var settings = context.CreatePermission(PermissionNames.Pages_Settings, L("Settings"));
			settings.CreateChildPermission(PermissionNames.Pages_Settings_Edit, L("EditSettings"));

			// ===== CONTENT MANAGEMENT =====
			// Banners
			var banners = context.CreatePermission(PermissionNames.Pages_Banners, L("Banners"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Create, L("CreateBanner"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Edit, L("EditBanner"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Delete, L("DeleteBanner"));

			// Sales/Promotions
			var sales = context.CreatePermission(PermissionNames.Pages_Sales, L("Sales"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Create, L("CreateSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Edit, L("EditSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Delete, L("DeleteSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_ToggleActive, L("ToggleActiveSale"));

			// ===== PRODUCT MANAGEMENT =====
			// Categories
			var category = context.CreatePermission(PermissionNames.Pages_categories, L("Categories"));
			category.CreateChildPermission(PermissionNames.Pages_category_create, L("CreateCategory"));
			category.CreateChildPermission(PermissionNames.Pages_category_update, L("UpdateCategory"));
			category.CreateChildPermission(PermissionNames.Pages_category_delete, L("DeleteCategory"));

			// Products
			var product = context.CreatePermission(PermissionNames.Pages_products, L("Products"));
			product.CreateChildPermission(PermissionNames.Pages_products_create, L("CreateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_update, L("UpdateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_delete, L("DeleteProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_search, L("SearchProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_view, L("ViewProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_import, L("ImportProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_export, L("ExportProduct"));

			// Product Variants
			var productVariants = context.CreatePermission(PermissionNames.Pages_productVariants, L("ProductVariants"));
			productVariants.CreateChildPermission(PermissionNames.Pages_productVariants_create, L("CreateProductVariant"));
			productVariants.CreateChildPermission(PermissionNames.Pages_productVariants_update, L("UpdateProductVariant"));
			productVariants.CreateChildPermission(PermissionNames.Pages_productVariants_delete, L("DeleteProductVariant"));
			productVariants.CreateChildPermission(PermissionNames.Pages_productVariants_view, L("ViewProductVariant"));

			// ===== SALES MANAGEMENT =====
			// Home Customer
			context.CreatePermission(PermissionNames.Pages_homeCustomer, L("HomeCustomer"));

			// Orders
			var orders = context.CreatePermission(PermissionNames.Pages_orders, L("Orders"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_create, L("CreateOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_view, L("ViewOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_edit, L("EditOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_delete, L("DeleteOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_confirm, L("ConfirmOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_cancel, L("CancelOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_ship, L("ShipOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_orders_complete, L("CompleteOrder"));

			// Shopping Carts
			var carts = context.CreatePermission(PermissionNames.Pages_carts, L("Carts"));
			carts.CreateChildPermission(PermissionNames.Pages_carts_addItem, L("AddToCart"));
			carts.CreateChildPermission(PermissionNames.Pages_carts_removeItem, L("RemoveFromCart"));
			carts.CreateChildPermission(PermissionNames.Pages_carts_view, L("ViewCart"));
			carts.CreateChildPermission(PermissionNames.Pages_carts_clear, L("ClearCart"));

			// ===== NOTIFICATIONS =====
			var notifications = context.CreatePermission(PermissionNames.Pages_notifications, L("Notifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_notifications_view, L("ViewNotifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_notifications_setAsRead, L("SetNotificationsAsRead"));
			notifications.CreateChildPermission(PermissionNames.Pages_notifications_delete, L("DeleteNotifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_notifications_settings, L("NotificationSettings"));
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}
