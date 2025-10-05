using Acme.SimpleTaskApp.Products.Dtos;
using Acme.SimpleTaskApp.Categories.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class DetailProductModalViewModel
	{
		public ProductListDto Product { get; set; } // Sản phẩm cụ thể
		public CategoryListDto Category { get; set; }

		public DetailProductModalViewModel(ProductListDto product)
		{
			Product = product;
		}
	}
}
