using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Acme.SimpleTaskApp.Authorization.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Acme.SimpleTaskApp.Carts
{
	[Table("AppCarts")]
	public class Cart : FullAuditedEntity<int>
	{
		public long UserId { get; set; }
	}
}