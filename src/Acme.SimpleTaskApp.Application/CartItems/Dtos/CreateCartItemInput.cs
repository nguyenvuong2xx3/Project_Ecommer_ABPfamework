using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems.Dtos
{
	public class CreateCartItemInput
	{
		public int CartId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
	}
}
