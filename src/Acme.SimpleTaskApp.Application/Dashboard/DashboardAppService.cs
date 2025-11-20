using Abp.Application.Services;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Orders;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Sales;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Dashboard
{
	public class DashboardAppService : ApplicationService, IDashboardAppService
	{
		private readonly IRepository<Order, int> _orderRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant, int> _variantRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Sale, int> _saleRepository;

		public DashboardAppService(
			IRepository<Order, int> orderRepository,
			IRepository<Product, int> productRepository,
			IRepository<ProductVariant, int> variantRepository,
			IRepository<User, long> userRepository,
			IRepository<Sale, int> saleRepository)
		{
			_orderRepository = orderRepository;
			_productRepository = productRepository;
			_variantRepository = variantRepository;
			_userRepository = userRepository;
			_saleRepository = saleRepository;
		}

		public async Task<DashboardStatsDto> GetDashboardStats()
		{
			var now = DateTime.Now;
			var thisMonth = new DateTime(now.Year, now.Month, 1);
			var lastMonth = thisMonth.AddMonths(-1);

			// Get orders data
			var orders = await _orderRepository.GetAll().ToListAsync();
			var thisMonthOrders = orders.Where(o => o.CreationTime >= thisMonth).ToList();
			var lastMonthOrders = orders.Where(o => o.CreationTime >= lastMonth && o.CreationTime < thisMonth).ToList();

			// Calculate revenue - Handle nullable decimals
			decimal totalRevenue = orders.Sum(o => o.TotalPrice ?? 0);
			decimal thisMonthRevenue = thisMonthOrders.Sum(o => o.TotalPrice ?? 0);
			decimal lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalPrice ?? 0);

			// Calculate growth
			decimal revenueGrowth = lastMonthRevenue > 0 
				? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
				: 0;

			int ordersGrowth = lastMonthOrders.Count > 0
				? (int)(((thisMonthOrders.Count - lastMonthOrders.Count) / (double)lastMonthOrders.Count) * 100)
				: 0;

			// Count products and customers
			var totalProducts = await _productRepository.CountAsync();
			var totalCustomers = await _userRepository.CountAsync();

			// Count pending and completed orders
			var pendingOrders = orders.Count(o => o.Status == 0);
			var completedOrders = orders.Count(o => o.Status == 2);

			// Calculate average order value
			decimal averageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0;

			// Count active vouchers
			var activeVouchers = await _saleRepository.GetAll()
				.Where(s => s.IsActive && 
					s.DiscountType == DiscountType.Voucher &&
					s.StartDate <= now && 
					s.EndDate >= now)
				.CountAsync();

			// Count low stock products
			var lowStockProducts = await _variantRepository.GetAll()
				.Where(v => v.StockQuantity < 10)
				.CountAsync();

			return new DashboardStatsDto
			{
				TotalRevenue = totalRevenue,
				TotalOrders = orders.Count,
				TotalProducts = totalProducts,
				TotalCustomers = totalCustomers,
				RevenueGrowth = revenueGrowth,
				OrdersGrowth = ordersGrowth,
				PendingOrders = pendingOrders,
				CompletedOrders = completedOrders,
				AverageOrderValue = averageOrderValue,
				ActiveVouchers = activeVouchers,
				LowStockProducts = lowStockProducts
			};
		}

		public async Task<List<ChartDataDto>> GetSalesChartData(int days = 30)
		{
			var startDate = DateTime.Now.AddDays(-days);
			var orders = await _orderRepository.GetAll()
				.Where(o => o.CreationTime >= startDate)
				.ToListAsync();

			var groupedData = orders
				.GroupBy(o => o.CreationTime.Date)
				.Select(g => new ChartDataDto
				{
					Label = g.Key.ToString("dd/MM"),
					Value = g.Sum(o => o.TotalPrice ?? 0)
				})
				.OrderBy(d => d.Label)
				.ToList();

			return groupedData;
		}

		public async Task<List<ChartDataDto>> GetOrdersChartData(int days = 30)
		{
			var startDate = DateTime.Now.AddDays(-days);
			var orders = await _orderRepository.GetAll()
				.Where(o => o.CreationTime >= startDate)
				.ToListAsync();

			var groupedData = orders
				.GroupBy(o => o.CreationTime.Date)
				.Select(g => new ChartDataDto
				{
					Label = g.Key.ToString("dd/MM"),
					Value = g.Count()
				})
				.OrderBy(d => d.Label)
				.ToList();

			return groupedData;
		}

		public async Task<List<TopProductDto>> GetTopSellingProducts(int count = 5)
		{
			var orders = await _orderRepository.GetAll().ToListAsync();

			// Deserialize order details
			foreach (var order in orders)
			{
				order.Deserialize();
			}

			// Group by product variant and count
			var topVariants = orders
				.SelectMany(o => o.OrderDetails)
				.GroupBy(od => od.ProductVariantId)
				.Select(g => new
				{
					VariantId = g.Key.Value,
					TotalSold = g.Sum(od => od.Quantity.Value),
					TotalRevenue = g.Sum(od => od.Quantity.Value * od.NewPrice.Value)
				})
				.OrderByDescending(x => x.TotalSold)
				.Take(count)
				.ToList();

			var result = new List<TopProductDto>();

			foreach (var item in topVariants)
			{
				var variant = await _variantRepository.FirstOrDefaultAsync(item.VariantId);
				if (variant == null)
					continue; // Skip if variant was deleted

				var product = await _productRepository.FirstOrDefaultAsync(variant.ProductId);
				if (product == null)
					continue; // Skip if product was deleted

				result.Add(new TopProductDto
				{
					ProductName = $"{product.Name} - {variant.Storage} - {variant.Color}",
					TotalSold = item.TotalSold,
					TotalRevenue = item.TotalRevenue,
					ImageUrl = variant.ImageUrl ?? "/img/products/default.png"
				});
			}

			return result;
		}

		public async Task<List<RecentOrderDto>> GetRecentOrders(int count = 10)
		{
			var orders = await _orderRepository.GetAll()
				.OrderByDescending(o => o.CreationTime)
				.Take(count)
				.ToListAsync();

			var result = orders.Select(o => new RecentOrderDto
			{
				OrderId = o.Id,
				OrderCode = o.Code,
				CustomerName = o.FullName ?? "Unknown",
				TotalPrice = o.TotalPrice ?? 0,
				Status = o.Status ?? 0,
				StatusText = GetStatusText(o.Status ?? 0),
				CreationTime = o.CreationTime
			}).ToList();

			return result;
		}

		public async Task<List<ChartDataDto>> GetRevenueByCategory()
		{
			var orders = await _orderRepository.GetAll().ToListAsync();

			// Deserialize order details
			foreach (var order in orders)
			{
				order.Deserialize();
			}

			// Get revenue by category - simplified without Category navigation
			var categoryRevenue = new Dictionary<int, decimal>();

			foreach (var order in orders)
			{
				if (order.OrderDetails != null)
				{
					foreach (var detail in order.OrderDetails)
					{
						if (detail.ProductVariantId.HasValue)
						{
							var variant = await _variantRepository.FirstOrDefaultAsync(detail.ProductVariantId.Value);
							if (variant != null)
							{
								var product = await _productRepository.FirstOrDefaultAsync(variant.ProductId);
								if (product != null && product.CategoryId.HasValue)
								{
									var categoryId = product.CategoryId.Value;
									var revenue = (detail.Quantity ?? 0) * (detail.NewPrice ?? 0);

									if (categoryRevenue.ContainsKey(categoryId))
										categoryRevenue[categoryId] += revenue;
									else
										categoryRevenue[categoryId] = revenue;
								}
							}
						}
					}
				}
			}

			var colors = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF", "#FF9F40" };
			var index = 0;

			return categoryRevenue
				.OrderByDescending(x => x.Value)
				.Select(x => new ChartDataDto
				{
					Label = "Category " + x.Key, // Simple label without navigation
					Value = x.Value,
					Color = colors[index++ % colors.Length]
				})
				.ToList();
		}

		private string GetStatusText(int status)
		{
			return status switch
			{
				0 => "Chờ xác nhận",
				1 => "Đã xác nhận",
				2 => "Hoàn thành",
				3 => "Đã hủy",
				4 => "Hoàn trả",
				_ => "Không xác định"
			};
		}
	}
}
