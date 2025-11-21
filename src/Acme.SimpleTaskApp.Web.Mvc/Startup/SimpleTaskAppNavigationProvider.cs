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

			// ===== QUẢN TRỊ HỆ THỐNG =====
			menu.AddItem(
				new MenuItemDefinition(
					"Administration",
					L("Administration"),
					icon: "fas fa-cogs"
				).AddItem(
					new MenuItemDefinition(
						PageNames.Tenants,
						L("Tenants"),
						url: "Tenants",
						icon: "fas fa-building",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
					)
				).AddItem(
					new MenuItemDefinition(
						PageNames.Users,
						L("Users"),
						url: "Users",
						icon: "fas fa-users",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
					)
				).AddItem(
					new MenuItemDefinition(
						PageNames.Roles,
						L("Roles"),
						url: "Roles",
						icon: "fas fa-theater-masks",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
					)
				).AddItem(
					new MenuItemDefinition(
						PageNames.Settings,
						L("Settings"),
						url: "Settings",
						icon: "fas fa-cog",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
					)
				)
			);

			// ===== QUẢN LÝ NỘI DUNG =====
			menu.AddItem(
				new MenuItemDefinition(
					"ContentManagement",
					L("ContentManagement"),
					icon: "fas fa-image"
				).AddItem(
					new MenuItemDefinition(
						PageNames.Banners,
						L("Banners"),
						url: "Banners",
						icon: "fas fa-images"
					)
				).AddItem(
					new MenuItemDefinition(
						PageNames.Sales,
						L("Sales"),
						url: "Sales",
						icon: "fas fa-tag"
					)
				)
			);

			// ===== QUẢN LÝ SẢN PHẨM =====
			menu.AddItem(
				new MenuItemDefinition(
					"ProductManagement",
					L("ProductManagement"),
					icon: "fas fa-boxes"
				).AddItem(
					new MenuItemDefinition(
						"Categories",
						L("Categories"),
						url: "Categories",
						icon: "fas fa-list",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_categories)
					)
				).AddItem(
					new MenuItemDefinition(
						"Products",
						L("Products"),
						url: "Products",
						icon: "fas fa-cube",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products_view)
					)
				).AddItem(
					new MenuItemDefinition(
						"ProductVariants",
						L("ProductVariants"),
						url: "ProductVariants",
						icon: "fas fa-cubes",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)
					)
				)
			);

			// ===== QUẢN LÝ BÁN HÀNG =====
			menu.AddItem(
				new MenuItemDefinition(
					"SalesManagement",
					L("SalesManagement"),
					icon: "fas fa-shopping-cart"
				).AddItem(
					new MenuItemDefinition(
						"HomeCustomer",
						L("HomeCustomer"),
						url: "HomeCustomer",
						icon: "fas fa-home",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_homeCustomer)
					)
				).AddItem(
					new MenuItemDefinition(
						"Orders",
						L("Orders"),
						url: "Orders",
						icon: "fas fa-box-open",
						permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_orders)
					)
				)
			);
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}