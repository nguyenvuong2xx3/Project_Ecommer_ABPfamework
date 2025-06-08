using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using static Acme.SimpleTaskApp.Products.Product;
using static Acme.SimpleTaskApp.Web.Views.Shared.Components.CategoryList.CategoryListViewModel;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class ProductViewModel
	{
		public IReadOnlyList<ProductListDto> Products { get; set; }
		public List<SelectListItem> Categories { get; set; }
		public int TotalPages { get; set; }
		public int PageNumber { get; set; }

		public string Keyword { get; set; }
		public string? Category { get; set; }

		public ProductViewModel(IReadOnlyList<ProductListDto> products)
		{
			Products = products;
		}
	}

}

