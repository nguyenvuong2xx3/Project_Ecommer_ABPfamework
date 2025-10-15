using Acme.SimpleTaskApp.CartItems.Dtos;
using System.Collections.Generic;
using System;
using Acme.SimpleTaskApp.Carts.Dtos;

namespace Acme.SimpleTaskApp.Web.Models.Carts
{
	public class CartViewModel
	{
		public List<CartDto> CartItems { get; set; }
	}
}
