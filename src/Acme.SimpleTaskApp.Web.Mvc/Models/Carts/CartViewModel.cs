using Acme.SimpleTaskApp.CartItems.Dtos;
using System.Collections.Generic;
using System;
using Acme.SimpleTaskApp.Carts.Dtos;
using Acme.SimpleTaskApp.Authorization.Users;

namespace Acme.SimpleTaskApp.Web.Models.Carts
{
	public class CartViewModel
	{
		public List<CartDto> CartItems { get; set; }
		public User User { get; set; }
		public string TinhThanh { get; set; }
		public string PhuongXa { get; set; }
	}
}
