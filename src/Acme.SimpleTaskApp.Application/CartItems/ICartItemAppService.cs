using Abp.Application.Services;
using Acme.SimpleTaskApp.CartItems.Dtos;
using Acme.SimpleTaskApp.Carts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems
{
	public interface ICartItemAppService : IApplicationService
	{
		Task<CartItemListDto> AddItemAsync(CreateCartItemInput input);
		Task<CartItemListDto> UpdateItemAsync(UpdateCartItemDto input);
		Task DeleteItemAsync(int productId, int cartId);
		//Task<CartItemListDto> GetItemAsync(int CartId);
	}

}
