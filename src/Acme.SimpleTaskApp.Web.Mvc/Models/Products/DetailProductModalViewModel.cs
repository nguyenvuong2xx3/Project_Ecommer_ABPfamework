using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

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
