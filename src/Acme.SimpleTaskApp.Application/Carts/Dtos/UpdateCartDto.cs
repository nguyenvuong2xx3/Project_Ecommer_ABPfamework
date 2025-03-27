using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
	public class UpdateCartDto : EntityDto, IHasCreationTime
	{
		public int UserId { get; set; }
		public DateTime CreationTime { get; set; }
	}
}
