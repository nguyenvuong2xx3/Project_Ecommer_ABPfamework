using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Products;
using Acme.SimpleTaskApp.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Carts
{
	public interface ICartFrontendAppService : IApplicationService
	{
		public Task<int> GetCartCountAsync();
	}

	public class CartFrontendAppService : ApplicationService, ICartFrontendAppService
	{
		private readonly IRepository<Cart, int> _cartRepository;
		private readonly IRepository<CartItem, int> _cartItemRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant> _productVariantRepository;

		public CartFrontendAppService(
				IRepository<Cart, int> cartRepository,
				IRepository<Product, int> productRepository,
				IRepository<ProductVariant> productVariantRepository,
				IRepository<CartItem, int> cartItemRepository)
		{
			_cartRepository = cartRepository;
			_productVariantRepository = productVariantRepository;
			_productRepository = productRepository;
			_cartItemRepository = cartItemRepository;
		}

		public async Task<int> GetCartCountAsync()
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
				try
				{
					var currentUserId = AbpSession.UserId ?? throw new Exception("Chưa đăng nhập");
					var cart = await _cartRepository.GetAll().FirstOrDefaultAsync(c => c.UserId == currentUserId);
					if (cart == null)
					{
						return 0;

					}
					var cartItems = (from cartItem in _cartItemRepository.GetAll().Where(ci => ci.CartId == cart.Id)
													 join productVariant in _productVariantRepository.GetAll() on cartItem.ProductVariantId equals productVariant.Id
													 join product in _productRepository.GetAll() on productVariant.ProductId equals product.Id
													 select new
													 {
														 Quantity = cartItem.Quantity
													 }).ToList();

					int cartItemCount = cartItems.Sum(ci => ci.Quantity);
					return cartItemCount;
				}
				catch (Exception)
				{
					return 0;
				}
				finally
				{
					await uow.CompleteAsync();
				}
		}

	}
}
