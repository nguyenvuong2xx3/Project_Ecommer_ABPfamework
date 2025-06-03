using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Carts.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Carts
{
	public interface ICartAppService : IApplicationService
	{
		Task<CartListDto> GetCart(GetCartInput input);
		Task CreateCart(int productId, int quantity);
		Task DeleteCart(long userId);
	}
}
