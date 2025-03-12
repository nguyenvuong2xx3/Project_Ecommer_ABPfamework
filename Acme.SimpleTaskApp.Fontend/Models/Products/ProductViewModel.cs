using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class ProductViewModel
	{
		public IReadOnlyList<ProductListDto> Products { get; set; }
		public List<SelectListItem> Categories { get; set; }

		public ProductViewModel(IReadOnlyList<ProductListDto> products)
		{
			Products = products;
		}
	}

}

