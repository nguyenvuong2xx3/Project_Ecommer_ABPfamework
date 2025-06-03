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

		public CartFrontendAppService(
				IRepository<Cart, int> cartRepository,
				IRepository<CartItem, int> cartItemRepository)
		{
			_cartRepository = cartRepository;
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
					var cartItemCount = await _cartItemRepository.GetAll()
							.Where(ci => ci.CartId == cart.Id)
							.SumAsync(ci => ci.Quantity);
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
