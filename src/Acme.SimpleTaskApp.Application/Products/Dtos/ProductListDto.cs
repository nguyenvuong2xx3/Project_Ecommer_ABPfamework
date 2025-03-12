using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Products.Dtos
{
	public class ProductListDto : EntityDto, IHasCreationTime
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public DateTime CreationTime { get; set; }

        public ProductState State { get; set; }

        public string Image { get; set; }  // Lưu đường dẫn ảnh

		    public string CategoryName { get; set; } // Lưu tên danh mục  
        
        public int? CategoryId { get; set; } // Lưu id danh mục

	}
}
