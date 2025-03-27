using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class DetailProductModalViewModel
	{
		public ProductListDto Product { get; set; } // Sản phẩm cụ thể
		public List<SelectListItem> Categories { get; set; } // Danh mục sản phẩm
		public List<ProductListDto> RelatedProducts { get; set; } // Tất cả sản phẩm khác

		public DetailProductModalViewModel(ProductListDto product, List<ProductListDto> allProducts)
		{
			Product = product;
			RelatedProducts = allProducts;
			Categories = new List<SelectListItem>();
		}
	}
}
