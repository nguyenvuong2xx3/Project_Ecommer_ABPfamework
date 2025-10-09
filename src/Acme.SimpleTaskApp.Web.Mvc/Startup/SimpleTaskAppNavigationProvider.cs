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

			// ===== QUẢN TRỊ HỆ THỐNG (Menu cha có submenu) =====
			menu.AddItem(
					new MenuItemDefinition(
							"Administration",
							L("Administration"),
							icon: "fas fa-cog"
					)
					.AddItem(
							new MenuItemDefinition(
									PageNames.Tenants,
									L("Tenants"),
									url: "Tenants",
									icon: "fas fa-building",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
							)
					)
					.AddItem(
							new MenuItemDefinition(
									PageNames.Users,
									L("Users"),
									url: "Users",
									icon: "fas fa-users",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
							)
					)
					.AddItem(
							new MenuItemDefinition(
									PageNames.Roles,
									L("Roles"),
									url: "Roles",
									icon: "fas fa-theater-masks",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
							)
					)
			);

			// ===== QUẢN LÝ SẢN PHẨM (Menu cha có submenu) =====
			menu.AddItem(
					new MenuItemDefinition(
							"CommodityManagement",
							L("CommodityManagement"),
							icon: "fas fa-box"
					)
					.AddItem(
							new MenuItemDefinition(
									"Categories",
									L("Categories"),
									url: "Categories",
									icon: "fa fa-list",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_categories)
							)
					)
					.AddItem(
							new MenuItemDefinition(
									"Products",
									L("Products"),
									url: "Products",
									icon: "fa fa-cart-plus",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)
							)
					).AddItem(
							new MenuItemDefinition(
									"ProductVariants",
									L("ProductVariants"),
									url: "ProductVariants",
									icon: "fa fa-cubes",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)
							)
					)
			);

			// ===== QUẢN LÝ ĐƠN HÀNG (Menu cha có submenu) =====
			menu.AddItem(
					new MenuItemDefinition(
							"OrderManagement",
							L("OrderManagement"),
							icon: "fas fa-shopping-cart"
					)
					.AddItem(
							new MenuItemDefinition(
									"HomeCustomer",
									L("HomeCustomer"),
									url: "HomeCustomer",
									icon: "fa fa-home",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_homeCustomer)
							)
					)
					.AddItem(
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