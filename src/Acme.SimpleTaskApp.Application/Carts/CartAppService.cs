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
using Acme.SimpleTaskApp.Sales;

public class CartAppService : ApplicationService, ICartAppService
{
	private readonly ISaleAppService _saleAppService;
	private readonly IRepository<Cart, int> _cartRepository;
	private readonly IRepository<Product, int> _productRepository;
	private readonly IRepository<ProductVariant, int> _productVariantRepository;
	private readonly IRepository<CartItem, int> _cartItemRepository;
	private readonly IRepository<ProductImage> _productImageRepository;
	public CartAppService(
		ISaleAppService saleAppService,
		IRepository<Cart, int> cartRepository, IRepository<CartItem, int> cartItemRepository,
		IRepository<ProductVariant, int> productVariantRepository,
		IRepository<ProductImage> productImageRepository,
		IRepository<Product, int> productRepository)
	{
		_saleAppService = saleAppService;
		_productImageRepository = productImageRepository;
		_productVariantRepository = productVariantRepository;
		_productRepository = productRepository;
		_cartRepository = cartRepository;
		_cartItemRepository = cartItemRepository;
	}

	[Authorize]
	public async Task CreateCart(int productvariantId, int quantity)
	{
		var userId = AbpSession.UserId;
		if (userId == null)
		{
			throw new Exception("Vui lòng đăng nhập để tiếp tục.");
		}

		// Kiểm tra stock khả dụng trước khi thêm vào giỏ hàng
		// AvailableStock = StockQuantity - ReservedQuantity (số lượng đang giữ chỗ bởi VNPay)
		var productVariant = await _productVariantRepository.FirstOrDefaultAsync(pv => pv.Id == productvariantId);
		if (productVariant == null)
		{
			throw new Exception("Sản phẩm không tồn tại.");
		}

		var cart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId);

		// Tính số lượng hiện có trong giỏ hàng của user (nếu có)
		int currentQuantityInCart = 0;
		if (cart != null)
		{
			var existingCartItem = await _cartItemRepository.FirstOrDefaultAsync(
				p => p.ProductVariantId == productvariantId && p.CartId == cart.Id);
			if (existingCartItem != null)
			{
				currentQuantityInCart = existingCartItem.Quantity;
			}
		}

		// Kiểm tra stock khả dụng
		var availableStock = productVariant.StockQuantity - productVariant.ReservedQuantity;
		var totalQuantityNeeded = currentQuantityInCart + quantity;

		if (availableStock < totalQuantityNeeded)
		{
			throw new Exception($"Sản phẩm chỉ còn {availableStock} sản phẩm khả dụng.");
		}

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
			p => p.ProductVariantId == productvariantId && p.CartId == cart.Id);

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
				ProductVariantId = productvariantId,
				Quantity = quantity
			};
			await _cartItemRepository.InsertAsync(cartItem);
		}
	}

	public async Task DeleteCart(long userId)
	{
		var getCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId);
		await _cartRepository.DeleteAsync(getCart.Id);
		var getAllCartItems = await _cartItemRepository.GetAllListAsync(c => c.CartId == getCart.Id);
		foreach (var item in getAllCartItems)
		{
			await _cartItemRepository.DeleteAsync(item.Id);
		}
	}

	public async Task<CartListDto> GetCart()
	{
		// 1. Lấy giỏ hàng và kiểm tra null
		var cart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId);

		if (cart == null)
		{
			// Có thể trả về null hoặc giỏ hàng rỗng tùy logic
			return new CartListDto { CartItems = new List<CartDto>() };
		}

		// Thay thế đoạn truy vấn cartItems trong phương thức GetCart bằng đoạn sau để chỉ lấy ảnh đầu tiên cho mỗi ProductVariant
		var cartItems = (from cartItem in _cartItemRepository.GetAll().Where(ci => ci.CartId == cart.Id)
										 join productVariant in _productVariantRepository.GetAll() on cartItem.ProductVariantId equals productVariant.Id
										 join product in _productRepository.GetAll() on productVariant.ProductId equals product.Id
										 // Lấy ảnh đầu tiên cho mỗi ProductVariant
										 let firstImage = _productImageRepository.GetAll()
												 .Where(img => img.ProductVariantId == productVariant.Id)
												 .OrderBy(img => img.SortOrder)
												 .FirstOrDefault()
										 select new CartDto
										 {
											 IdCart = cart.Id,
											 IdCartItem = cartItem.Id,
											 IdProductVariant = productVariant.Id,
											 ProductId = product.Id,
											 CategoryId = product.CategoryId,
											 Name = product.Name + " - " + productVariant.Storage + " - " + productVariant.Color,
											 Color = productVariant.Color,
											 Quantity = cartItem.Quantity,
											 Price = productVariant.Price,
											 ImageUrl = firstImage != null ? firstImage.ImageUrl : null,
											 DiscountPercentage = 0,
											 DiscountedPrice = productVariant.Price,
											 HasActiveDiscount = false
										 }).ToList();

		// Calculate discounts for each cart item
		foreach (var cartItem in cartItems)
		{
			var productVariant = await _productVariantRepository.GetAsync(cartItem.IdProductVariant);
			var product = await _productRepository.GetAsync(cartItem.ProductId);

			// Get best sale for this variant
			var bestSale = await _saleAppService.GetBestSaleForProductVariant(
				cartItem.IdProductVariant,
				cartItem.ProductId,
				cartItem.CategoryId
			);

			if (bestSale != null && bestSale.DiscountPercentage > 0)
			{
				cartItem.HasActiveDiscount = true;
				cartItem.DiscountPercentage = bestSale.DiscountPercentage;
				cartItem.DiscountedPrice = cartItem.Price - (cartItem.Price * (bestSale.DiscountPercentage / 100));
				cartItem.SaleName = bestSale.Name;
			}
		}

		return new CartListDto { CartItems = cartItems };
	}
}
