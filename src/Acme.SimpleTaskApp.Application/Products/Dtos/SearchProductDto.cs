using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Products.Dtos
{
	public class SearchProductDto : PagedAndSortedResultRequestDto
	{
		public string SearchTerm { get; set; }
		public string SKU { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Screen { get; set; }
		public string Processor { get; set; }
		public string CameraSystem { get; set; }
		public string Battery { get; set; }
		public int? CategoryId { get; set; }
		public int? StockQuantityFrom { get; set; }
		public int? StockQuantityTo { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	}
}
