using Microsoft.AspNetCore.Http;
using System;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Products.Dtos
{
    public class CreateProductDto
	{
        public string Name { get; set; }
        
        public int? CategoryId { get; set; }

		    public string Description { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        public IFormFile ImageFile { get; set; }

        public ProductState State { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
