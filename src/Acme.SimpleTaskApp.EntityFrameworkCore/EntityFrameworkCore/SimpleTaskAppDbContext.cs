﻿using Abp.Zero.EntityFrameworkCore;
using Acme.SimpleTaskApp.Authorization.Roles;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.MultiTenancy;
using Acme.SimpleTaskApp.People;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Tasks;
using Microsoft.EntityFrameworkCore;
using Acme.SimpleTaskApp.AppTours;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.OrderItems;

namespace Acme.SimpleTaskApp.EntityFrameworkCore
{
	public class SimpleTaskAppDbContext : AbpZeroDbContext<Tenant, Role, User, SimpleTaskAppDbContext>
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Categories.Category> Categories { get; set; }
        public DbSet<Tour> AppTours { get; set; }
		    public DbSet<Cart> Carts  { get; set; }
        public DbSet<CartItem> CartItems  { get; set; }
		    public DbSet<Order> Orders { get; set; }
		    public DbSet<OrderDetails> OrderDetails { get; set; }




		public SimpleTaskAppDbContext(DbContextOptions<SimpleTaskAppDbContext> options)
            : base(options)
        {

        }
    }
}