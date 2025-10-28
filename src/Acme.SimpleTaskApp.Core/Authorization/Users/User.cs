using System;
using System.Collections.Generic;
using Abp.Authorization.Users;
using Abp.Extensions;

namespace Acme.SimpleTaskApp.Authorization.Users
{
	public class User : AbpUser<User>
	{
		public const string DefaultPassword = "123qwe";

		public string? DiaChiChiTiet { get; set; }
		public string? TinhThanh { get; set; }
		public string? PhuongXa { get; set; }
		public int? GioiTinh { get; set; } // 0: Nu, 1: Nam, 2: Khac
		public int? IsDiaChiMacDinh { get; set; }

		public static string CreateRandomPassword()
		{
			return Guid.NewGuid().ToString("N").Truncate(16);
		}

		public static User CreateTenantAdminUser(int tenantId, string emailAddress)
		{
			var user = new User
			{
				TenantId = tenantId,
				UserName = AdminUserName,
				Name = AdminUserName,
				Surname = AdminUserName,
				EmailAddress = emailAddress,
				Roles = new List<UserRole>()
			};

			user.SetNormalizedNames();

			return user;
		}
	}
}
