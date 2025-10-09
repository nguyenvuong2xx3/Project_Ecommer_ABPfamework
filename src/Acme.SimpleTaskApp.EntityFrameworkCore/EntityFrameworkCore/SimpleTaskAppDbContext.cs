using Abp.Zero.EntityFrameworkCore;
using Acme.SimpleTaskApp.Authorization.Roles;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.MultiTenancy;
using Acme.SimpleTaskApp.OrderItems;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;

namespace Acme.SimpleTaskApp.EntityFrameworkCore
{
	public class SimpleTaskAppDbContext : AbpZeroDbContext<Tenant, Role, User, SimpleTaskAppDbContext>
	{
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductImage> ProductImages { get; set; }
		public DbSet<Products.ProductVariant> ProductVariants { get; set; }
		public DbSet<Categories.Category> Categories { get; set; }

		//public DbSet<Cart> Carts { get; set; }
		//public DbSet<CartItem> CartItems { get; set; }
		//public DbSet<Order> Orders { get; set; }
		//public DbSet<OrderDetails> OrderDetails { get; set; }

		public SimpleTaskAppDbContext(DbContextOptions<SimpleTaskAppDbContext> options)
						: base(options)
		{

		}
	}
}