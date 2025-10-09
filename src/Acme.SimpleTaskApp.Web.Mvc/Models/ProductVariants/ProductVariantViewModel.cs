using System.Collections.Generic;
using Acme.SimpleTaskApp.Products;

namespace Acme.SimpleTaskApp.Web.Models.ProductVariants
{
	public class ProductVariantViewModel
	{
		public List<ProductVariant> ProductVariants { get; set; }
		public ProductVariant ProductVariant { get; set; }
		public List<Product> Products { get; set; }
	}
}
