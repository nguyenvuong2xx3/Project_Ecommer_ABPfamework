using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems
{

	[Table("AppCartItem")]
	public class CartItem : Entity
	{
		[Required]

		public int ProductId { get; set; }

		public int Quantity { get; set; }

		public decimal Price { get; set; }

		public decimal TotalPrice => Quantity * Price;


	}
}
