using Abp.Application.Services.Dto;
using System;

namespace Acme.SimpleTaskApp.ProductVariants.Dtos
{
	public class GetProductVariantsInput : PagedAndSortedResultRequestDto
	{
		public int? ProductId { get; set; }

		// Các trường tìm kiếm cơ bản và nâng cao
		public string SearchTerm { get; set; }
		public string ProductName { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Color { get; set; }
		public string SKU { get; set; }
		public decimal? MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
		public int? StockQuantity { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}