using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Locations.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.Orders
{
	public class OrderInfoModalViewModel
	{
		public List<TinhThanhDto> TinhThanh { get; set; }
		public User User { get; set; }
	}
}
