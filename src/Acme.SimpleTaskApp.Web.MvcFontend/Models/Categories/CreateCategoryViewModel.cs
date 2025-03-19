using Acme.SimpleTaskApp.Categories.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Categories.Dto
{
	public class CreateCategoryViewModel
    {
        public CreateCategoryDto Category { get; set; } // Sửa lại để binding dữ liệu

        public CreateCategoryViewModel(List<SelectListItem> categories = null)
        {
			        Category = new CreateCategoryDto();
        }
    }
}
