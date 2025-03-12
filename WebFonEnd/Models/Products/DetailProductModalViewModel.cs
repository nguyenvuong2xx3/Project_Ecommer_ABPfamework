using Acme.SimpleTaskApp.Categories.Dto;
using Acme.SimpleTaskApp.Products.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
    public class DetailCategoryModalViewModel
    {
            public ProductListDto Product { get; set; }
            public CategoryListDto Category { get; }

    }
}
