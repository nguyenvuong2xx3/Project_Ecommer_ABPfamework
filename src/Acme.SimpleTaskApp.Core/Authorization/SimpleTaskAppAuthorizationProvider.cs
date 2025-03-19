using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Acme.SimpleTaskApp.Authorization
{
	public class SimpleTaskAppAuthorizationProvider : AuthorizationProvider
	{
		public override void SetPermissions(IPermissionDefinitionContext context)
		{
			context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
			context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
			context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
			context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

			var product = context.CreatePermission(PermissionNames.Pages_products, L("Products"));
			product.CreateChildPermission(PermissionNames.Pages_products_create, L("CreateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_update, L("UpdateProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_delete, L("DeleteProduct"));
			product.CreateChildPermission(PermissionNames.Pages_products_search, L("SearchProduct"));


			var category = context.CreatePermission(PermissionNames.Pages_categories, L("Categories"));
			category.CreateChildPermission(PermissionNames.Pages_category_update, L("UpdateCategory"));
			category.CreateChildPermission(PermissionNames.Pages_category_delete, L("DeleteCategory"));
			category.CreateChildPermission(PermissionNames.Pages_category_create, L("CreateCategory"));

			var homeCustomer = context.CreatePermission(PermissionNames.Pages_homeCustomer, L("HomeCustomer"));

		}

		private static ILocalizableString L(string name)
		{
			return new LocalizableString(name, SimpleTaskAppConsts.LocalizationSourceName);
		}
	}
}
