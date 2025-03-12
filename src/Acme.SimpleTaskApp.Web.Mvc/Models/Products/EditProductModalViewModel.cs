using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Acme.SimpleTaskApp.Products.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
	public class EditProductModalViewModel
	{
		public UpdateProductDto Product { get; set; }
		public List<SelectListItem> Categories { get; set; }
	}
}
