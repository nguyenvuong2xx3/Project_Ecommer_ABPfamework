using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Orders.Dtos
{
	public class OrderListDto
	{
		public List<CreateOrderInput> Orders { get; set; }
		public int TotalCount { get; set; }
	}
}
