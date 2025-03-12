using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Products.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
    public class DetailProductModalViewModel
	{
            public ProductListDto Product { get; set; }
            public CategoryListDto Category { get; set; }
						public List<CategoryListDto> Categories { get; set; }

	}
}
