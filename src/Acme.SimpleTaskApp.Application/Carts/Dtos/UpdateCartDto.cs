using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Acme.SimpleTaskApp.Carts.Cart;

namespace Acme.SimpleTaskApp.Carts.Dtos
{
    public class UpdateCartDto : EntityDto, IHasCreationTime
    {
		public int UserId { get; set; }
		public int Count { get; set; }
		public DateTime CreationTime { get; set; }
		public int IdCartItem { get; set; }
	}
}
