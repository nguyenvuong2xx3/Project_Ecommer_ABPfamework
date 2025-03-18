using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Categories.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.CategoryList
{
	public class CategoryListViewModel
	{
		public ListResultDto<CategoryListDto> Categories { get; set; }

		public class CategoryDto
		{
			public string Name { get; set; }
			public string Description { get; set; }
		}
	}
}
