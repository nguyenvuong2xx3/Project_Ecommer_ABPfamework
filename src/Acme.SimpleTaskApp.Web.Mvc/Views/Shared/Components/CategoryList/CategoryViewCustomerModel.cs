using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Categories.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.CategoryList
{
	public class CategoryViewCustomerModel
	{
		public List<CategoryTreeDto> Categories { get; set; }
	}
}
