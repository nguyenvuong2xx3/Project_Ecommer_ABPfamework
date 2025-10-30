using Acme.SimpleTaskApp.OrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class CreateOrderInput
	{
		public Order Order { get; set; }
		public List<OrderDetails> OrderDetails { get; set; }
	}
}
