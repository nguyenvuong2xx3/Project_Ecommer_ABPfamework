using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class ProductViewModel
	{
		public List<Category> Categories { get; set; }
		public Category Category { get; set; }
		public Product Product { get; set; }
	}
}

