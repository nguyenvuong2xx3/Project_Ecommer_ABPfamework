using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class OrdersDto
	{
		public List<OrderDetailDto> OrderDetails { get; set; }

		public int PaymentMethod { get; set; } // Payment method used for the order


	}
}
