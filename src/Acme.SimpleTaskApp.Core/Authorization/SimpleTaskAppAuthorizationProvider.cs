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
			var tenants = context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
			//tenants.CreateChildPermission(PermissionNames.Pages_Tenants_View, L("ViewTenants"));

			// Users
			var users = context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
			users.CreateChildPermission(PermissionNames.Pages_Users_View, L("ViewUsers"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Create, L("CreateNewUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Edit, L("EditUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Delete, L("DeleteUser"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
			users.CreateChildPermission(PermissionNames.Pages_Users_Impersonation, L("LoginForUsers"));
			users.CreateChildPermission(PermissionNames.Pages_Users_ChangePermissions, L("ChangePermissions"));

			// Roles
			var roles = context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_View, L("ViewRoles"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Create, L("CreateNewRole"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Edit, L("EditRole"));
			roles.CreateChildPermission(PermissionNames.Pages_Roles_Delete, L("DeleteRole"));

			// Settings
			var settings = context.CreatePermission(PermissionNames.Pages_Settings, L("Settings"));
			settings.CreateChildPermission(PermissionNames.Pages_Settings_View, L("ViewSettings"));
			settings.CreateChildPermission(PermissionNames.Pages_Settings_Edit, L("EditSettings"));

			// ===== CONTENT MANAGEMENT =====
			// Banners
			var banners = context.CreatePermission(PermissionNames.Pages_Banners, L("Banners"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_View, L("ViewBanners"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Create, L("CreateBanner"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Edit, L("EditBanner"));
			banners.CreateChildPermission(PermissionNames.Pages_Banners_Delete, L("DeleteBanner"));

			// Sales/Promotions
			var sales = context.CreatePermission(PermissionNames.Pages_Sales, L("Sales"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_View, L("ViewSales"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Create, L("CreateSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Edit, L("EditSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_Delete, L("DeleteSale"));
			sales.CreateChildPermission(PermissionNames.Pages_Sales_ToggleActive, L("ToggleActiveSale"));

			// Categories
			var category = context.CreatePermission(PermissionNames.Pages_Categories, L("Categories"));
			category.CreateChildPermission(PermissionNames.Pages_Categories_View, L("ViewCategories"));
			category.CreateChildPermission(PermissionNames.Pages_Categories_Create, L("CreateCategory"));
			category.CreateChildPermission(PermissionNames.Pages_Categories_Update, L("UpdateCategory"));
			category.CreateChildPermission(PermissionNames.Pages_Categories_Delete, L("DeleteCategory"));

			// Products
			var product = context.CreatePermission(PermissionNames.Pages_Products, L("Products"));
			product.CreateChildPermission(PermissionNames.Pages_Products_View, L("ViewProducts"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Create, L("CreateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Update, L("UpdateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Delete, L("DeleteProduct"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Search, L("SearchProduct"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Import, L("ImportProduct"));
			product.CreateChildPermission(PermissionNames.Pages_Products_Export, L("ExportProduct"));

			// Product Variants
			var productVariants = context.CreatePermission(PermissionNames.Pages_ProductVariants, L("ProductVariants"));
			productVariants.CreateChildPermission(PermissionNames.Pages_ProductVariants_View, L("ViewProductVariants"));
			productVariants.CreateChildPermission(PermissionNames.Pages_ProductVariants_Create, L("CreateProductVariant"));
			productVariants.CreateChildPermission(PermissionNames.Pages_ProductVariants_Update, L("UpdateProductVariant"));
			productVariants.CreateChildPermission(PermissionNames.Pages_ProductVariants_Delete, L("DeleteProductVariant"));

			// ===== SALES MANAGEMENT =====
			// Home Customer
			context.CreatePermission(PermissionNames.Pages_HomeCustomer, L("HomeCustomer"));

			// Orders
			var orders = context.CreatePermission(PermissionNames.Pages_Orders, L("Orders"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_View, L("ViewOrders"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Create, L("CreateOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Edit, L("EditOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Delete, L("DeleteOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Confirm, L("ConfirmOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Cancel, L("CancelOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Ship, L("ShipOrder"));
			orders.CreateChildPermission(PermissionNames.Pages_Orders_Complete, L("CompleteOrder"));

			// Shopping Carts
			var carts = context.CreatePermission(PermissionNames.Pages_Carts, L("Carts"));
			carts.CreateChildPermission(PermissionNames.Pages_Carts_View, L("ViewCarts"));
			carts.CreateChildPermission(PermissionNames.Pages_Carts_AddItem, L("AddToCart"));
			carts.CreateChildPermission(PermissionNames.Pages_Carts_RemoveItem, L("RemoveFromCart"));
			carts.CreateChildPermission(PermissionNames.Pages_Carts_Clear, L("ClearCart"));

			// ===== NOTIFICATIONS =====
			var notifications = context.CreatePermission(PermissionNames.Pages_Notifications, L("Notifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_Notifications_View, L("ViewNotifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_Notifications_SetAsRead, L("SetNotificationsAsRead"));
			notifications.CreateChildPermission(PermissionNames.Pages_Notifications_Delete, L("DeleteNotifications"));
			notifications.CreateChildPermission(PermissionNames.Pages_Notifications_Settings, L("NotificationSettings"));
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}
