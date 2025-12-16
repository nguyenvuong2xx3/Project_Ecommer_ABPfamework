using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Acme.SimpleTaskApp.CartItems.Dtos;
using Acme.SimpleTaskApp.Products;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.CartItems
{
	public class CartItemAppService : ApplicationService
	{
		private readonly IRepository<CartItem, int> _cartItemRepository;
		private readonly IRepository<ProductVariant, int> _productVariantRepository;

		public CartItemAppService(
			IRepository<CartItem, int> cartItemRepository,
			IRepository<ProductVariant, int> productVariantRepository)
		{
			_cartItemRepository = cartItemRepository;
			_productVariantRepository = productVariantRepository;
		}

		// Cập nhật số lượng sản phẩm trong giỏ hàng
		public async Task UpdateItemAsync(UpdateCartItemDto input)
		{
			// Tìm CartItem theo ProductId và CartId
			var cartItem = await _cartItemRepository.FirstOrDefaultAsync(x =>
					x.ProductVariantId == input.ProductVariantId &&
					x.CartId == input.CartId
			);

			if (cartItem == null) throw new UserFriendlyException("Sản phẩm không tồn tại trong giỏ hàng.");

			// Kiểm tra stock khả dụng trước khi cập nhật
			// AvailableStock = StockQuantity - ReservedQuantity (số lượng đang giữ chỗ bởi VNPay)
			var productVariant = await _productVariantRepository.FirstOrDefaultAsync(pv => pv.Id == input.ProductVariantId);
			if (productVariant == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại.");
			}

			var availableStock = productVariant.StockQuantity - productVariant.ReservedQuantity;
			if (availableStock < input.Quantity)
			{
				throw new UserFriendlyException($"Sản phẩm chỉ còn {availableStock} sản phẩm khả dụng.");
			}

			// Cập nhật quantity
			cartItem.Quantity = input.Quantity;

			await _cartItemRepository.UpdateAsync(cartItem);
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


		public async Task DeleteItemAsync(int productVariantId, int cartId)
		{
			var cartItem = await _cartItemRepository.FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductVariantId == productVariantId);
			if (cartItem == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại");
			}
			await _cartItemRepository.DeleteAsync(cartItem);
		}
	}
}

