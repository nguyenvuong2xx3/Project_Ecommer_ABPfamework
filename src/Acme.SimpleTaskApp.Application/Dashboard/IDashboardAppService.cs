using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Dashboard
{
	public interface IDashboardAppService : IApplicationService
	{
		Task<DashboardStatsDto> GetDashboardStats();
		Task<List<ChartDataDto>> GetSalesChartData(int days = 30);
		Task<List<ChartDataDto>> GetOrdersChartData(int days = 30);
		Task<List<TopProductDto>> GetTopSellingProducts(int count = 5);
		Task<List<RecentOrderDto>> GetRecentOrders(int count = 10);
		Task<List<ChartDataDto>> GetRevenueByCategory();
	}

	public class DashboardStatsDto
	{
		public decimal TotalRevenue { get; set; }
		public int TotalOrders { get; set; }
		public int TotalProducts { get; set; }
		public int TotalCustomers { get; set; }
		public decimal RevenueGrowth { get; set; }
		public int OrdersGrowth { get; set; }
		public int PendingOrders { get; set; }
		public int CompletedOrders { get; set; }
		public decimal AverageOrderValue { get; set; }
		public int ActiveVouchers { get; set; }
		public int LowStockProducts { get; set; }
	}

	public class ChartDataDto
	{
		public string Label { get; set; }
		public decimal Value { get; set; }
		public string Color { get; set; }
	}

	public class TopProductDto
	{
		public string ProductName { get; set; }
		public int TotalSold { get; set; }
		public decimal TotalRevenue { get; set; }
		public string ImageUrl { get; set; }
	}

	public class RecentOrderDto
	{
		public int OrderId { get; set; }
		public string OrderCode { get; set; }
		public string CustomerName { get; set; }
		public decimal TotalPrice { get; set; }
		public int Status { get; set; }
		public string StatusText { get; set; }
		public DateTime CreationTime { get; set; }
	}
}
