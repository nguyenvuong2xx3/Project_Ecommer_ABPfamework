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
			context.Manager.MainMenu
					.AddItem(
							new MenuItemDefinition(
									PageNames.About,
									L("About"),
									url: "About",
									icon: "fas fa-info-circle"
							)
					).AddItem(
							new MenuItemDefinition(
									PageNames.Home,
									L("HomePage"),
									url: "",
									icon: "fas fa-home",
									requiresAuthentication: true
							)
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
					)
					.AddItem(
							new MenuItemDefinition(
									"Quản lý mẫu sản phẩm",
									L("Products"),
									url: "Products",
									icon: "fa fa-cart-plus",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)

							)
					)
					.AddItem(
							new MenuItemDefinition(
									"Quản lý sản phẩm",
									L("ProductVariant"),
									url: "ProductVariant",
									icon: "fa fa-cart-plus",
									permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_products)
							)
					).AddItem(
							new MenuItemDefinition(
											"Categories",
											L("Categories"),
											url: "Categories",
											icon: "fa fa-list",
											permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_categories)

							)
					).AddItem(
							new MenuItemDefinition(
											"HomeCustomer",
											L("HomeCustomer"),
											url: "HomeCustomer",
											icon: "fa fa-globe",
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
					);
					//.AddItem(
					//		new MenuItemDefinition(
					//						"Tours",
					//						L("Tours"),
					//						url: "Tours",
					//						icon: "fa fa-globe"
					//		//permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_categories)

					//		)
					//);
		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}