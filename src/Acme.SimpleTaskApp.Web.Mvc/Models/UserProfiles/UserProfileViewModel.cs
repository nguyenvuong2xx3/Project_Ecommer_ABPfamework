using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Locations.Dtos;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Web.Models.UserProfiles
{
	public class UserProfileViewModel
	{
		public User User { get; set; }
		public string? TenTinhThanh { get; set; }
		public string? TenPhuongXa { get; set; }
		//public List<TinhThanhDto> TinhThanhDto { get; set; }
	}
}
