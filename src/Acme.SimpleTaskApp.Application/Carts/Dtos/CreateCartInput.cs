using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using static Acme.SimpleTaskApp.Carts.Cart;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
    public class CreateCartInput
	  {
			[Required]
			public int UserId { get; set; }
			public int Count { get; set; }
			public DateTime CreationTime { get; set; }
			public int IdCartItem { get; set; }

	}
}
