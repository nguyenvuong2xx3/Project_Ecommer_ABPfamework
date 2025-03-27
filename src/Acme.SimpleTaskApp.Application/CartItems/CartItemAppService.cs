using Abp.Application.Services;
using Abp.Domain.Repositories;
using Acme.SimpleTaskApp.CartItems.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems
{
	public class CartItemAppService : ApplicationService
	{
		private readonly IRepository<CartItem, int> _cartItemRepository;

		public CartItemAppService( IRepository<CartItem, int> cartItemRepository)
		{
			_cartItemRepository = cartItemRepository;
		}

		//tao moi
		public async Task<CartItemListDto> AddItemAsync(CreateCartItemInput input)
		{
			var cartItem = new CartItem
			{
				CartId = input.CartId,
				ProductId = input.ProductId,
				Quantity = input.Quantity,
			};

			await _cartItemRepository.InsertAsync(cartItem);

			return new CartItemListDto
			{
				ProductId = cartItem.ProductId,
				Quantity = cartItem.Quantity,
			};
		}



		//cap nhat
		public async Task<CartItemListDto> UpdateItemAsync(UpdateCartItemDto input)
		{
			// Tìm CartItem theo ProductId và CartId
			var cartItem = await _cartItemRepository.FirstOrDefaultAsync(x =>
					x.ProductId == input.ProductId &&
					x.CartId == input.CartId
			);

			if (cartItem == null) throw new Exception("Item not found");

			// Cập nhật quantity
			cartItem.Quantity = input.Quantity;

			await _cartItemRepository.UpdateAsync(cartItem);

			return new CartItemListDto
			{
				ProductId = cartItem.ProductId,
				Quantity = cartItem.Quantity,

			};
		}



		//public async Task<CartItemListDto> GetItemAsync(int cartId)
		//{
		//	var cartItem = await _cartItemRepository.FirstOrDefaultAsync(x => x.CartId == cartId);
		//	if (cartItem == null)
		//	{
		//		throw new Exception("Item not found");
		//	}

		//	return new CartItemListDto
		//	{
		//		ProductId = cartItem.ProductId,
		//		Quantity = cartItem.Quantity,
		//	};
		//}


		public async Task DeleteItemAsync(int productId, int cartId)
		{
			var cartItem = await _cartItemRepository.FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId);
			if (cartItem == null)
			{
				throw new Exception("Sản phẩm không tồn tại");
			}
			await _cartItemRepository.DeleteAsync(cartItem);
		}
	}
}

