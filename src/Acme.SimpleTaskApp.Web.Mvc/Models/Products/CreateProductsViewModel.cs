using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
    public class CreateProductViewModel
	{
			public List<Category> Categories { get; set; }
			public string ProductName { get; set; }
			public string ProductDescription { get; set; }
			public int? CategoryId { get; set; }
	}
}
