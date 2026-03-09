using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Categories;
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
		private readonly IRepository<ProductImage, int> _productImageRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant, int> _productvariantRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Sale, int> _saleRepository;
		private readonly IRepository<Category, int> _categoryRepository;

		public DashboardAppService(
			IRepository<ProductImage, int> productImageRepository,
		IRepository<Order, int> orderRepository,
			IRepository<Product, int> productRepository,
			IRepository<ProductVariant, int> productvariantRepository,
			IRepository<User, long> userRepository,
			IRepository<Sale, int> saleRepository,
			IRepository<Category, int> categoryRepository
			)
		{
			_productImageRepository = productImageRepository;
			_categoryRepository = categoryRepository;
			_orderRepository = orderRepository;
			_productRepository = productRepository;
			_productvariantRepository = productvariantRepository;
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

			// Count low stock products - Tính stock khả dụng = StockQuantity - ReservedQuantity
			var lowStockProducts = await _productvariantRepository.GetAll()
				.Where(v => (v.StockQuantity - v.ReservedQuantity) < 3)
				.CountAsync();

			return new DashboardStatsDto
			{
				TotalRevenue = totalRevenue, // tổng doanh thu
				TotalOrders = orders.Count, // tổng số đơn hàng
				TotalProducts = totalProducts, // tổng số sản phẩm
				TotalCustomers = totalCustomers, // tổng số khách hàng
				RevenueGrowth = revenueGrowth, // tăng trưởng doanh thu
				OrdersGrowth = ordersGrowth, // tăng trưởng đơn hàng
				PendingOrders = pendingOrders, // đơn hàng chờ xử lý
				CompletedOrders = completedOrders, // đơn hàng hoàn thành
				AverageOrderValue = averageOrderValue, // giá trị trung bình đơn hàng
				ActiveVouchers = activeVouchers, // voucher đang hoạt động
				LowStockProducts = lowStockProducts // sản phẩm tồn kho thấp
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
				var variant = await _productvariantRepository.FirstOrDefaultAsync(item.VariantId);
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
			var categoryRevenue = new Dictionary<string, decimal>();

			foreach (var order in orders)
			{
				if (order.OrderDetails != null)
				{
					foreach (var detail in order.OrderDetails)
					{
						if (detail.ProductVariantId.HasValue)
						{
							var variant = await _productvariantRepository.FirstOrDefaultAsync(detail.ProductVariantId.Value);
							if (variant != null)
							{
								var product = await _productRepository.FirstOrDefaultAsync(variant.ProductId);
								if (product != null && product.CategoryId.HasValue)
								{
									var categoryId = product.CategoryId.Value;
									var categoryName = _categoryRepository.FirstOrDefaultAsync(x => x.Id == categoryId).Result.Name;
									var revenue = (detail.Quantity ?? 0) * (detail.NewPrice ?? 0);

									if (categoryRevenue.ContainsKey(categoryName))
										categoryRevenue[categoryName] += revenue;
									else
										categoryRevenue[categoryName] = revenue;
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
					Label = x.Key, // Simple label without navigation
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

		public async Task<PagedResultDto<LowStockProductDto>> GetLowStockProducts(PagedAndSortedResultRequestDto input)
		{
			// Sử dụng biểu thức SQL-translatable thay vì NotMapped property
			// AvailableStock = StockQuantity - ReservedQuantity
			var productVariants = await _productvariantRepository.GetAll()
				.Where(v => (v.StockQuantity - v.ReservedQuantity) < 3)
				.ToListAsync();

			var result = new List<LowStockProductDto>();

			foreach (var variant in productVariants)
			{
				var product = await _productRepository.FirstOrDefaultAsync(variant.ProductId);
				if (product == null)
					continue;

				var productImage = await _productImageRepository.FirstOrDefaultAsync(x => x.ProductVariantId == variant.Id);

				result.Add(new LowStockProductDto
				{
					ProductId = product.Id,
					ProductName = product.Name,
					ImageUrl = productImage?.ImageUrl ?? variant.ImageUrl ?? "/img/products/default.png",
					VariantId = variant.Id,
					Storage = variant.Storage,
					Ram = variant.Ram,
					Color = variant.Color,
					StockQuantity = variant.StockQuantity,
					ReservedQuantity = variant.ReservedQuantity,
					AvailableStock = variant.StockQuantity - variant.ReservedQuantity,
				});
			}

			var totalCount = result.Count;
			var pagedResult = result
				.Skip(input.SkipCount)
				.Take(input.MaxResultCount)
				.ToList();

			return new PagedResultDto<LowStockProductDto>(totalCount, pagedResult);
		}
	}
	public class LowStockProductDto
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ImageUrl { get; set; }
		public int VariantId { get; set; }
		public string Storage { get; set; }
		public string Ram { get; set; }
		public string Color { get; set; }
		public int StockQuantity { get; set; }
		public int ReservedQuantity { get; set; }
		public int AvailableStock { get; set; }
	}
}
