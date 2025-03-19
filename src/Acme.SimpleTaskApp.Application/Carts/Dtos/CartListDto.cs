using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;
using static Acme.SimpleTaskApp.Carts.Cart;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
	public class CartListDto : EntityDto, IHasCreationTime
	{

		public int UserId { get; set; }
		public int Count { get; set; }
		public DateTime CreationTime { get; set; }
		public int IdCartItem { get; set; }


	}
}
