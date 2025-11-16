using System;

namespace Acme.SimpleTaskApp.ProductVariants.Dtos
{
	public class ProductVariantListDto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Color { get; set; }
		public decimal Price { get; set; }
		public string SKU { get; set; }
	}
}
