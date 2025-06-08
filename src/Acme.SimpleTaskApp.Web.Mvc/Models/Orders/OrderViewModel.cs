using Acme.SimpleTaskApp.Orders.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Orders
{
	public class OrderViewModel
	{
		public List<OrderDetailDto> OrderDetails { get; set; }

		public int PaymentMethod { get; set; }

		public string UserName { get; set; }
		public string EmailAddress { get; set; }
		public int Status { get; set; }

		public decimal TotalPrice { get; set; }
	}
}
