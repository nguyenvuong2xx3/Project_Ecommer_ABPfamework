using Abp.Application.Services.Dto;

namespace Acme.SimpleTaskApp.ProductVariants.Dtos
{
	public class GetProductVariantsInput : PagedAndSortedResultRequestDto
	{
		public int? ProductId { get; set; }
		public string Keyword { get; set; }
		public int? MinPrice { get; set; }
		public int? MaxPrice { get; set; }
		public int? MinStock { get; set; }
		public int? MaxStock { get; set; }
		public string Color { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }

	}
}