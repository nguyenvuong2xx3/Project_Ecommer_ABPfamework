using System.Collections.Generic;
using System.Linq;
using Acme.SimpleTaskApp.Orders.Dtos;
using Acme.SimpleTaskApp.Products.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Orders
{
	public class DetailOrderViewModel
	{
		public List<OrderDetailDto> OrderList { get; set; }
		public List<ProductListDto> ProductList { get; set; }

		public ProductListDto GetProductById(int productId)
		{
			return ProductList?.FirstOrDefault(p => p.Id == productId);
		}
	}
}
