using Abp.Domain.Repositories;
using System.Threading.Tasks;
using System;
using Acme.SimpleTaskApp.Carts;
using Acme.SimpleTaskApp.Carts.Dtos;
using System.Linq;
using Abp.Application.Services;
using Acme.SimpleTaskApp.CartItems.Dtos;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Authorization.Users;
using Microsoft.AspNetCore.Authorization;

public class CartAppService : ApplicationService, ICartAppService
{
	private readonly IRepository<Cart, int> _cartRepository;
	private readonly IRepository<Product, int> _productRepository;
	private readonly IRepository<CartItem, int> _cartItemRepository;

	public CartAppService(IRepository<Cart, int> cartRepository, IRepository<CartItem, int> cartItemRepository, 
		IRepository<Product, int> productRepository)
	{
		_productRepository = productRepository;
		_cartRepository = cartRepository;
		_cartItemRepository = cartItemRepository;

	}


	public async Task CreateCart(int productId, int quantity)
	{
		var userId = AbpSession.UserId;
		if (userId == null)
		{
			throw new Exception("Vui lòng đăng nhập để tiếp tục.");
		}

		var cart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId);
		if (cart == null)
		{
			cart = new Cart
			{
				UserId = (long)userId,
				CreationTime = DateTime.Now
			};
			await _cartRepository.InsertAsync(cart);
			await CurrentUnitOfWork.SaveChangesAsync(); // đảm bảo cart.Id có giá trị
		}

		// Sửa điều kiện kiểm tra sản phẩm
		var checkProduct = await _cartItemRepository.FirstOrDefaultAsync(
			p => p.ProductId == productId && p.CartId == cart.Id);

		if (checkProduct != null)
		{
			checkProduct.Quantity += quantity;
			await _cartItemRepository.UpdateAsync(checkProduct);
		}
		else
		{
			var cartItem = new CartItem
			{
				CartId = cart.Id,
				ProductId = productId,
				Quantity = quantity
			};
			await _cartItemRepository.InsertAsync(cartItem);
		}
	}


	public async Task DeleteCart(int userId)
	{
		await _cartRepository.DeleteAsync(userId);

	}

	public async Task<CartListDto> GetCart(GetCartInput input)
	{
		// 1. Lấy giỏ hàng và kiểm tra null
		var cart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == input.UserId);

		if (cart == null)
		{
			// Có thể trả về null hoặc giỏ hàng rỗng tùy logic
			return new CartListDto { CartItems = new List<CartItemListDto>() };
		}

		// 2. Thực hiện join để lấy productName theo productId
		var cartItems = await (from cartItem in _cartItemRepository.GetAll()
													 join product in _productRepository.GetAll() on cartItem.ProductId equals product.Id
													 where cartItem.CartId == cart.Id
													 select new CartItemListDto
													 {
														 ProductId = cartItem.ProductId,
														 CartId = cartItem.CartId,
														 ProductName = product.Name,
														 Quantity = cartItem.Quantity,
														 Price = product.Price,
														 Image = product.Image
													 }).ToListAsync();

		var result = new CartListDto
		{
			UserId = cart.UserId,
			CreationTime = cart.CreationTime,
			CartItems = cartItems
		};

		return result;
	}

}
