using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Products.Dtos
{
    public class SearchProductDto
	{
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public DateTime CreationTime { get; set; }

        public ProductState State { get; set; }

        public string Keyword { get; set; }

        public string Image { get; set; }  // Lưu đường dẫn ảnh
    }
}
