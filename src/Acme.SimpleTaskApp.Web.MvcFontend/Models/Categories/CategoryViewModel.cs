using Acme.SimpleTaskApp.Categories.Dto;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Categories
{
	public class CategoryViewModel
	{
		public IReadOnlyList<CategoryListDto> Category;

		public CategoryViewModel(IReadOnlyList<CategoryListDto> category)
		{
			Category = category;
		}

	}
}
