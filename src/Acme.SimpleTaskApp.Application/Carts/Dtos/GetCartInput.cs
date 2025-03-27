using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.CartItems.Dtos;
using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
	public class GetCartInput : PagedAndSortedResultRequestDto
	{
		public long UserId { get; set; }

		public DateTime CreationTime { get; set; }

		public int ProductId { get; set; }

		public int Quantity { get; set; }

		public decimal Price { get; set; }

	}
}
