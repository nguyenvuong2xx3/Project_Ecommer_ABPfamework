using Acme.SimpleTaskApp.Categories.Dtos;
using System.Collections.Generic;
using Acme.SimpleTaskApp.Categories;

namespace Acme.SimpleTaskApp.Web.Models.Categories
{
	public class EditCategoryModalViewModel
	{
		public EditCategoryDto Category { get; set; }
		public List<Category> Categories { get; set; }
	}
}
