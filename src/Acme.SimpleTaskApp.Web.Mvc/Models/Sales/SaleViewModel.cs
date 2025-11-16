using Acme.SimpleTaskApp.Categories.Dtos;
using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.ProductVariants.Dtos;
using Acme.SimpleTaskApp.Sales.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Sales
{
	public class SaleViewModel
	{
		public SaleDto Sale { get; set; }

		// Danh sách để hiển thị trong dropdown/tagify
		public List<CategoryListDto> Categories { get; set; }
		public List<ProductListDto> Products { get; set; }
		public List<ProductVariantListDto> ProductVariants { get; set; }
	}
}
