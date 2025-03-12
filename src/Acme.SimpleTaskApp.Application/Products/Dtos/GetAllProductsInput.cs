using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Products.Dtos
{
    public class GetAllProductsInput : PagedAndSortedResultRequestDto
    {
        public string Name { get; set; } // Tìm kiếm theo tên sản phẩm

        public ProductState? State { get; set; } // Lọc theo trạng thái sản phẩm

        public DateTime CreateTime { get; set; }

        public string Category { get; set; }

		    public string StateInput { get; set; }

        public string Keyword { get; set; }

        public bool? IsActive { get; set; }
    }
}
