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
		
		/// <summary>
		/// Mã voucher (nếu có) - sẽ được validate và apply khi tạo order
		/// </summary>
		public string VoucherCode { get; set; }
	}
}
