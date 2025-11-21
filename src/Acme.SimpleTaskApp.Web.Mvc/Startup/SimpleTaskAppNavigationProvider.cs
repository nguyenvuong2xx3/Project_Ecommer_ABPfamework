using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Web.Startup
{
	/// <summary>
	/// This class defines menus for the application.
	/// </summary>
	public class SimpleTaskAppNavigationProvider : NavigationProvider
	{
		public override void SetNavigation(INavigationProviderContext context)
		{
			var menu = context.Manager.MainMenu;

			// ===== DASHBOARD =====
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Home,
					L("HomePage"),
					url: "Home",
					icon: "fas fa-tachometer-alt"
				)
			);

			// ===== QUẢN TRỊ HỆ THỐNG - Flatten items =====
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Tenants,
					L("Tenants"),
					url: "Tenants",
					icon: "fas fa-building",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Users,
					L("Users"),
					url: "Users",
					icon: "fas fa-users",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Roles,
					L("Roles"),
					url: "Roles",
					icon: "fas fa-theater-masks",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Settings,
					L("Settings"),
					url: "Settings",
					icon: "fas fa-cog",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Banners,
					L("Banners"),
					url: "Banners",
					icon: "fas fa-image"
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					PageNames.Sales,
					L("Sales"),
					url: "Sales",
					icon: "fas fa-tag"
				)
			);

			// ===== QUẢN LÝ SẢN PHẨM - Flatten items =====
			menu.AddItem(
				new MenuItemDefinition(
					"Categories",
					L("Categories"),
					url: "Categories",
					icon: "fa fa-list",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_categories)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					"Products",
					L("Products"),
					url: "Products",
					icon: "fa fa-cart-plus",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products_view)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					"ProductVariants",
					L("ProductVariants"),
					url: "ProductVariants",
					icon: "fa fa-cubes",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)
				)
			);

			// ===== QUẢN LÝ ĐƠN HÀNG - Flatten items =====
			menu.AddItem(
				new MenuItemDefinition(
					"HomeCustomer",
					L("HomeCustomer"),
					url: "HomeCustomer",
					icon: "fa fa-home",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_homeCustomer)
				)
			);
			
			menu.AddItem(
				new MenuItemDefinition(
					"Orders",
					L("Orders"),
					url: "Orders",
					icon: "fas fa-box-open",
					permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_orders)
				)
			);
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}