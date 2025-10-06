using Acme.SimpleTaskApp.Categories;
using Acme.SimpleTaskApp.Products.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Products
{
    public class CreateProductViewModel
    {
        public CreateProductDto Product { get; set; }
        public List<Category> Categories { get; set; }

        public CreateProductViewModel()
        {
            Product = new CreateProductDto();
            Categories = new List<Category>();
        }
    }
}