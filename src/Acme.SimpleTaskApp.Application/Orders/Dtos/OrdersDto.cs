using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class OrdersDto
	{
		public string OrderDate { get; set; } // Date when the order was placed
		public List<OrderDetailDto> OrderDetails { get; set; }

		public int PaymentMethod { get; set; } 

		public string UserName { get; set; }
		public string EmailAddress { get; set; }
		public int Status { get; set; }

		public decimal TotalPrice { get; set; }

	}
}
