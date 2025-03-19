using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using static Acme.SimpleTaskApp.Products.Product;

namespace Acme.SimpleTaskApp.Web.Views.Shared.Components.ProductList
{
	public class ProductListViewModel
	{
		public ListResultDto<ProductListDto> RelatedProducts { get; set; }

		public class ProductDto
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public decimal Price { get; set; }
			public DateTime CreationTime { get; set; }
			public ProductState ProductState { get; set; }
			public string Image { get; set; }  // Lưu đường dẫn ảnh
		}
	}
}
