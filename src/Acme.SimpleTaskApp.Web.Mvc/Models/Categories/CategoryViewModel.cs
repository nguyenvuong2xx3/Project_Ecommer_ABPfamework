using Acme.SimpleTaskApp.Categories;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Categories
{
	public class CategoryViewModel
	{
		public List<Category> Categories { get; set; }
		public Category Category { get; set; }
	}
}
