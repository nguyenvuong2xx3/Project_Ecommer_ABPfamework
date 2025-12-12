using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.UI;
using Acme.SimpleTaskApp.Sales.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Acme.SimpleTaskApp.Authorization;

namespace Acme.SimpleTaskApp.Sales
{
	public class SaleAppService : ApplicationService, ISaleAppService
	{
		private readonly IRepository<Sale, int> _saleRepository;

		public SaleAppService(IRepository<Sale, int> saleRepository)
		{
			_saleRepository = saleRepository;
		}

		#region CRUD Operations

		// Tạo mới Sale
		[AbpAuthorize(PermissionNames.Pages_Sales_Create)]
		public async Task<SaleDto> CreateSale(CreateSaleDto input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không hợp lệ");
			}

			// Validate dates
			if (input.EndDate <= input.StartDate)
			{
				throw new UserFriendlyException("Ngày kết thúc phải sau ngày bắt đầu");
			}

			// Validate VoucherCode nếu là loại Voucher
			if (input.DiscountType == DiscountType.Voucher)
			{
				if (string.IsNullOrWhiteSpace(input.VoucherCode))
				{
					throw new UserFriendlyException("Mã voucher không được để trống khi loại giảm giá là Voucher");
				}

				// Check duplicate voucher code
				var existingVoucher = await _saleRepository.FirstOrDefaultAsync(s => s.VoucherCode == input.VoucherCode);
				if (existingVoucher != null)
				{
					throw new UserFriendlyException($"Mã voucher '{input.VoucherCode}' đã tồn tại");
				}
			}

			// Validate ApplyTo vs IDs
			ValidateApplyToConfiguration(input.ApplyTo, input.CategoryIds, input.ProductIds, input.ProductVariantIds);

			// Tạo đối tượng Sale
			Sale sale = new Sale
			{
				Name = input.Name,
				Description = input.Description,
				DiscountType = input.DiscountType,
				VoucherCode = input.VoucherCode?.Trim().ToUpper(),
				UsageLimit = input.UsageLimit,
				UsedCount = 0,
				StartDate = input.StartDate,
				EndDate = input.EndDate,
				DiscountPercentage = input.DiscountPercentage,
				MaximumDiscountAmount = input.MaximumDiscountAmount,
				MinimumOrderValue = input.MinimumOrderValue,
				ApplyTo = input.ApplyTo,
				CategoryIds = input.CategoryIds,
				ProductIds = input.ProductIds,
				ProductVariantIds = input.ProductVariantIds,
				IsActive = input.IsActive
			};

			// Insert vào database
			var result = await _saleRepository.InsertAsync(sale);
			await CurrentUnitOfWork.SaveChangesAsync();

			return MapToDto(result);
		}

		// Cập nhật Sale
		[AbpAuthorize(PermissionNames.Pages_Sales_Edit)]
		public async Task<SaleDto> UpdateSale(UpdateSaleDto input)
		{
			if (input == null)
			{
				throw new UserFriendlyException("Dữ liệu không hợp lệ");
			}

			var sale = await _saleRepository.FirstOrDefaultAsync(input.Id);
			if (sale == null)
				throw new UserFriendlyException($"Không tìm thấy Sale có Id = {input.Id}");

			// Validate dates
			if (input.EndDate <= input.StartDate)
			{
				throw new UserFriendlyException("Ngày kết thúc phải sau ngày bắt đầu");
			}

			// Validate VoucherCode nếu là loại Voucher
			if (input.DiscountType == DiscountType.Voucher)
			{
				if (string.IsNullOrWhiteSpace(input.VoucherCode))
				{
					throw new UserFriendlyException("Mã voucher không được để trống khi loại giảm giá là Voucher");
				}

				// Check duplicate voucher code (exclude current sale)
				var existingVoucher = await _saleRepository.FirstOrDefaultAsync(s =>
					s.VoucherCode == input.VoucherCode && s.Id != input.Id);
				if (existingVoucher != null)
				{
					throw new UserFriendlyException($"Mã voucher '{input.VoucherCode}' đã tồn tại");
				}
			}

			// Validate ApplyTo vs IDs
			ValidateApplyToConfiguration(input.ApplyTo, input.CategoryIds, input.ProductIds, input.ProductVariantIds);

			// Cập nhật các thuộc tính
			sale.Name = input.Name;
			sale.Description = input.Description;
			sale.DiscountType = input.DiscountType;
			sale.VoucherCode = input.VoucherCode?.Trim().ToUpper();
			sale.UsageLimit = input.UsageLimit;
			sale.StartDate = input.StartDate;
			sale.EndDate = input.EndDate;
			sale.DiscountPercentage = input.DiscountPercentage;
			sale.MaximumDiscountAmount = input.MaximumDiscountAmount;
			sale.MinimumOrderValue = input.MinimumOrderValue;
			sale.ApplyTo = input.ApplyTo;
			sale.CategoryIds = input.CategoryIds;
			sale.ProductIds = input.ProductIds;
			sale.ProductVariantIds = input.ProductVariantIds;
			sale.IsActive = input.IsActive;

			await _saleRepository.UpdateAsync(sale);
			await CurrentUnitOfWork.SaveChangesAsync();

			return MapToDto(sale);
		}

		// Xóa Sale
		[AbpAuthorize(PermissionNames.Pages_Sales_Delete)]
		public async Task DeleteSale(int id)
		{
			var sale = await _saleRepository.FirstOrDefaultAsync(id);
			if (sale == null)
				throw new UserFriendlyException($"Không tìm thấy Sale có Id = {id}");

			// Check if sale has been used
			if (sale.UsedCount > 0)
			{
				throw new UserFriendlyException($"Không thể xóa chương trình giảm giá đã được sử dụng {sale.UsedCount} lần. Bạn có thể vô hiệu hóa thay vì xóa.");
			}

			await _saleRepository.DeleteAsync(sale);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		// Lấy Sale theo ID
		[AbpAuthorize(PermissionNames.Pages_Sales_View)]
		public async Task<SaleDto> GetSaleById(int id)
		{
			if (id <= 0)
			{
				throw new UserFriendlyException("Id không hợp lệ");
			}

			var sale = await _saleRepository.FirstOrDefaultAsync(id);
			if (sale == null)
			{
				throw new UserFriendlyException($"Không tìm thấy Sale có Id = {id}");
			}

			return MapToDto(sale);
		}

		// Lấy danh sách Sale có phân trang và lọc
		[AbpAuthorize(PermissionNames.Pages_Sales_View)]
		public async Task<PagedResultDto<SaleDto>> GetAllSales(GetAllSaleDto input)
		{
			var query = _saleRepository.GetAll();

			// Lọc theo từ khóa (Name hoặc VoucherCode)
			if (!string.IsNullOrWhiteSpace(input.Filter))
			{
				var f = input.Filter.Trim().ToLower();
				query = query.Where(x =>
					x.Name.ToLower().Contains(f) ||
					(x.VoucherCode != null && x.VoucherCode.ToLower().Contains(f)));
			}

			// Lọc theo loại giảm giá
			if (input.DiscountType.HasValue)
				query = query.Where(x => x.DiscountType == input.DiscountType.Value);

			// Lọc theo phạm vi áp dụng
			if (input.ApplyTo.HasValue)
				query = query.Where(x => x.ApplyTo == input.ApplyTo.Value);

			// Lọc theo thời gian bắt đầu
			if (input.StartDateFrom.HasValue)
				query = query.Where(x => x.StartDate >= input.StartDateFrom.Value);

			if (input.StartDateTo.HasValue)
				query = query.Where(x => x.StartDate <= input.StartDateTo.Value);

			// Lọc theo thời gian kết thúc
			if (input.EndDateFrom.HasValue)
				query = query.Where(x => x.EndDate >= input.EndDateFrom.Value);

			if (input.EndDateTo.HasValue)
				query = query.Where(x => x.EndDate <= input.EndDateTo.Value);

			// Lọc theo phần trăm giảm giá
			if (input.MinDiscountPercentage.HasValue)
				query = query.Where(x => x.DiscountPercentage >= input.MinDiscountPercentage.Value);

			if (input.MaxDiscountPercentage.HasValue)
				query = query.Where(x => x.DiscountPercentage <= input.MaxDiscountPercentage.Value);

			// Lọc theo trạng thái
			if (input.IsActive.HasValue)
				query = query.Where(x => x.IsActive == input.IsActive.Value);

			// Lọc theo mã voucher
			if (!string.IsNullOrWhiteSpace(input.VoucherCode))
				query = query.Where(x => x.VoucherCode == input.VoucherCode.Trim().ToUpper());

			// Lọc theo đã hết lượt sử dụng
			if (input.HasUsageLimitReached.HasValue && input.HasUsageLimitReached.Value)
				query = query.Where(x => x.UsageLimit.HasValue && x.UsedCount >= x.UsageLimit.Value);

			// Tổng số sale
			var totalCount = await query.CountAsync();
			var result = await query
				.OrderByDescending(c => c.CreationTime)
				.PageBy(input)
				.ToListAsync();

			var dtos = result.Select(s => MapToDto(s)).ToList();

			return new PagedResultDto<SaleDto>(totalCount, dtos);
		}

		#endregion

		#region Voucher Operations
		[AbpAuthorize(PermissionNames.Pages_Sales_ToggleActive)]
		public async Task<Sale> ActiveOrInActive(int id)
		{
			if (id <= 0)
			{
				throw new UserFriendlyException("Id không hợp lệ");
			}

			var sale = await _saleRepository.FirstOrDefaultAsync(id);
			if (sale == null)
			{
				throw new UserFriendlyException($"Không tìm thấy Sale có Id = {id}");
			}

			// Toggle trạng thái
			sale.IsActive = !sale.IsActive;

			await _saleRepository.UpdateAsync(sale);

			return sale;
		}


		// Lấy Sale theo mã Voucher
		public async Task<SaleDto> GetSaleByVoucherCode(string voucherCode)
		{
			if (string.IsNullOrWhiteSpace(voucherCode))
			{
				throw new UserFriendlyException("Mã voucher không hợp lệ");
			}

			var sale = await _saleRepository.FirstOrDefaultAsync(s =>
				s.VoucherCode == voucherCode.Trim().ToUpper() &&
				s.DiscountType == DiscountType.Voucher);

			if (sale == null)
			{
				throw new UserFriendlyException("Mã voucher không tồn tại");
			}

			return MapToDto(sale);
		}

		// Validate mã Voucher
		public async Task<bool> ValidateVoucherCode(string voucherCode, decimal orderValue)
		{
			if (string.IsNullOrWhiteSpace(voucherCode))
			{
				return false;
			}

			var sale = await _saleRepository.FirstOrDefaultAsync(s =>
				s.VoucherCode == voucherCode.Trim().ToUpper() &&
				s.DiscountType == DiscountType.Voucher &&
				s.IsActive);

			if (sale == null)
			{
				return false;
			}

			// Check time validity
			var now = DateTime.Now;
			if (now < sale.StartDate || now > sale.EndDate)
			{
				return false;
			}

			// Check usage limit
			if (sale.UsageLimit.HasValue && sale.UsedCount >= sale.UsageLimit.Value)
			{
				return false;
			}

			// Check minimum order value
			if (sale.MinimumOrderValue.HasValue && orderValue < sale.MinimumOrderValue.Value)
			{
				return false;
			}

			return true;
		}

		// Tăng số lần đã sử dụng
		public async Task IncrementUsedCount(int saleId)
		{
			var sale = await _saleRepository.FirstOrDefaultAsync(saleId);
			if (sale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình giảm giá");
			}

			sale.UsedCount++;
			await _saleRepository.UpdateAsync(sale);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#endregion

		#region Check Operations

		// Kiểm tra Sale có đang hoạt động
		public async Task<bool> IsSaleActive(int id)
		{
			var sale = await _saleRepository.FirstOrDefaultAsync(id);
			if (sale == null || !sale.IsActive)
			{
				return false;
			}

			var now = DateTime.Now;
			if (now < sale.StartDate || now > sale.EndDate)
			{
				return false;
			}

			// Check usage limit
			if (sale.UsageLimit.HasValue && sale.UsedCount >= sale.UsageLimit.Value)
			{
				return false;
			}

			return true;
		}

		// Kiểm tra Sale có áp dụng cho đơn hàng
		public async Task<bool> IsSaleApplicableForOrder(int saleId, decimal orderValue)
		{
			var sale = await _saleRepository.FirstOrDefaultAsync(saleId);
			if (sale == null)
			{
				return false;
			}

			// Check active status
			if (!await IsSaleActive(saleId))
			{
				return false;
			}

			// Check minimum order value
			if (sale.MinimumOrderValue.HasValue && orderValue < sale.MinimumOrderValue.Value)
			{
				return false;
			}

			return true;
		}

		// Lấy các Sale đang hoạt động cho Product
		public async Task<List<SaleDto>> GetActiveSalesForProduct(int productId)
		{
			var now = DateTime.Now;
			var sales = await _saleRepository.GetAll()
				.Where(s => s.IsActive &&
					s.StartDate <= now &&
					s.EndDate >= now &&
					(s.ApplyTo == DiscountApplication.EntireOrder ||
					 (s.ApplyTo == DiscountApplication.Products && s.ProductIds.Contains(productId))))
				.OrderByDescending(s => s.DiscountPercentage)
				.ToListAsync();

			return sales.Select(s => MapToDto(s)).ToList();
		}

		// Lấy các Sale đang hoạt động cho Category
		public async Task<List<SaleDto>> GetActiveSalesForCategory(int categoryId)
		{
			var now = DateTime.Now;
			var sales = await _saleRepository.GetAll()
				.Where(s => s.IsActive &&
					s.StartDate <= now &&
					s.EndDate >= now &&
					(s.ApplyTo == DiscountApplication.EntireOrder ||
					 (s.ApplyTo == DiscountApplication.Categories && s.CategoryIds.Contains(categoryId))))
				.OrderByDescending(s => s.DiscountPercentage)
				.ToListAsync();

			return sales.Select(s => MapToDto(s)).ToList();
		}

		// Lấy các Sale đang hoạt động cho Variant
		public async Task<List<SaleDto>> GetActiveSalesForVariant(int variantId)
		{
			var now = DateTime.Now;
			var sales = await _saleRepository.GetAll()
				.Where(s => s.IsActive &&
					s.StartDate <= now &&
					s.EndDate >= now &&
					(s.ApplyTo == DiscountApplication.EntireOrder ||
					 (s.ApplyTo == DiscountApplication.Variants && s.ProductVariantIds.Contains(variantId))))
				.OrderByDescending(s => s.DiscountPercentage)
				.ToListAsync();

			return sales.Select(s => MapToDto(s)).ToList();
		}

		// Lấy các Sale tự động đang hoạt động
		public async Task<List<SaleDto>> GetActiveAutomaticSales()
		{
			var now = DateTime.Now;
			var sales = await _saleRepository.GetAll()
				.Where(s => s.IsActive &&
					s.DiscountType == DiscountType.Automatic &&
					s.StartDate <= now &&
					s.EndDate >= now &&
					(!s.UsageLimit.HasValue || s.UsedCount < s.UsageLimit.Value))
				.OrderByDescending(s => s.DiscountPercentage)
				.ToListAsync();

			return sales.Select(s => MapToDto(s)).ToList();
		}

		#endregion

		#region Statistics

		// Lấy thống kê của Sale
		public async Task<Dictionary<string, object>> GetSaleStatistics(int saleId)
		{
			var sale = await _saleRepository.FirstOrDefaultAsync(saleId);
			if (sale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình giảm giá");
			}

			var stats = new Dictionary<string, object>
			{
				{ "Id", sale.Id },
				{ "Name", sale.Name },
				{ "UsedCount", sale.UsedCount },
				{ "UsageLimit", sale.UsageLimit ?? 0 },
				{ "RemainingUsage", sale.UsageLimit.HasValue ? (sale.UsageLimit.Value - sale.UsedCount) : int.MaxValue },
				{ "UsagePercentage", sale.UsageLimit.HasValue ? (sale.UsedCount * 100.0 / sale.UsageLimit.Value) : 0 },
				{ "IsActive", sale.IsActive },
				{ "Status", GetSaleStatus(sale) },
				{ "DaysRemaining", (sale.EndDate - DateTime.Now).Days }
			};

			return stats;
		}

		#endregion

		#region Calculate Discount Methods

		// Tính discount cho một product variant
		public async Task<decimal> CalculateProductVariantDiscount(int productVariantId, int productId, int? categoryId)
		{
			var bestSale = await GetBestSaleForProductVariant(productVariantId, productId, categoryId);
			return bestSale?.DiscountPercentage ?? 0;
		}

		// Lấy sale tốt nhất cho product variant
		[AbpAllowAnonymous]
		public async Task<SaleDto> GetBestSaleForProductVariant(int productVariantId, int productId, int? categoryId)
		{
			var now = DateTime.Now;

			// Lấy tất cả sale đang active
			var activeSales = await _saleRepository.GetAll()
				.Where(s => s.IsActive &&
					s.DiscountType == DiscountType.Automatic && // Chỉ lấy automatic sales
					s.StartDate <= now &&
					s.EndDate >= now &&
					(!s.UsageLimit.HasValue || s.UsedCount < s.UsageLimit.Value))
				.ToListAsync();

			// Lọc sales áp dụng cho variant này
			var applicableSales = activeSales.Where(s =>
			{
				switch (s.ApplyTo)
				{
					case DiscountApplication.EntireOrder:
						return true;
					case DiscountApplication.Variants:
						return s.ProductVariantIds != null && s.ProductVariantIds.Contains(productVariantId);
					case DiscountApplication.Products:
						return s.ProductIds != null && s.ProductIds.Contains(productId);
					case DiscountApplication.Categories:
						return categoryId.HasValue && s.CategoryIds != null && s.CategoryIds.Contains(categoryId.Value);
					default:
						return false;
				}
			}).ToList();

			// Trả về sale có % giảm giá cao nhất
			var bestSale = applicableSales.OrderByDescending(s => s.DiscountPercentage).FirstOrDefault();

			return bestSale != null ? MapToDto(bestSale) : null;
		}

		// Tính discount cho giỏ hàng
		public async Task<DiscountCalculationResultDto> CalculateCartDiscount(RequestDisCountVoucherCart request)
		{
			var result = new DiscountCalculationResultDto
			{
				ItemDiscounts = new List<ItemDiscountDetailDto>(),
				IsSuccess = true
			};

			decimal totalAmount = 0;
			foreach (var item in request.CartItems)
			{
				totalAmount += item.Quantity * item.Price;
			}
			decimal totalDiscount = 0;

			// Tính discount cho từng item
			//foreach (var item in request.CartItems)
			//{
			//	var itemTotal = item.Price * item.Quantity;
			//	totalAmount += itemTotal;

			//	// Lấy sale tốt nhất cho item này
			//	var bestSale = await GetBestSaleForProductVariant(item.ProductVariantId, item.ProductId, item.CategoryId);

			//	if (bestSale != null)
			//	{
			//		var discountAmount = itemTotal * (bestSale.DiscountPercentage / 100);
			//		totalDiscount += discountAmount;

			//		result.ItemDiscounts.Add(new ItemDiscountDetailDto
			//		{
			//			ProductVariantId = item.ProductVariantId,
			//			OriginalPrice = item.Price,
			//			DiscountPercentage = bestSale.DiscountPercentage,
			//			DiscountAmount = discountAmount,
			//			FinalPrice = item.Price - (item.Price * (bestSale.DiscountPercentage / 100)),
			//			AppliedSale = bestSale
			//		});
			//	}
			//}

			//result.TotalAmount = totalAmount;

			// Apply voucher nếu có
			if (!string.IsNullOrWhiteSpace(request.VoucherCode))
			{
				try
				{
					var voucher = await _saleRepository.FirstOrDefaultAsync(s =>
						s.VoucherCode == request.VoucherCode.Trim().ToUpper() &&
						s.DiscountType == DiscountType.Voucher &&
						s.IsActive);

					if (voucher == null)
					{
						result.IsSuccess = false;
						result.Message = "Mã voucher không tồn tại hoặc đã hết hạn";
						result.FinalAmount = totalAmount - totalDiscount;
						return result;
					}

					// Validate voucher
					var now = DateTime.Now;
					if (now < voucher.StartDate || now > voucher.EndDate)
					{
						result.IsSuccess = false;
						result.Message = $"Mã voucher chỉ có hiệu lực từ {voucher.StartDate:dd/MM/yyyy} đến {voucher.EndDate:dd/MM/yyyy}";
						result.FinalAmount = totalAmount - totalDiscount;
						return result;
					}

					if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
					{
						result.IsSuccess = false;
						result.Message = "Mã voucher đã hết lượt sử dụng";
						result.FinalAmount = totalAmount - totalDiscount;
						return result;
					}

					if (voucher.MinimumOrderValue.HasValue && totalAmount < voucher.MinimumOrderValue.Value)
					{
						result.IsSuccess = false;
						result.Message = $"Đơn hàng tối thiểu phải từ {voucher.MinimumOrderValue.Value:N0}đ để áp dụng voucher này";
						result.FinalAmount = totalAmount - totalDiscount;
						return result;
					}

					// Check if voucher applies to cart items
					bool voucherApplies = false;
					switch (voucher.ApplyTo)
					{
						case DiscountApplication.EntireOrder:
							voucherApplies = true;
							break;
						case DiscountApplication.Categories:
							voucherApplies = request.CartItems.Any(item =>
								item.CategoryId.HasValue &&
								voucher.CategoryIds != null &&
								voucher.CategoryIds.Contains(item.CategoryId.Value));
							break;
						case DiscountApplication.Products:
							voucherApplies = request.CartItems.Any(item =>
								voucher.ProductIds != null &&
								voucher.ProductIds.Contains(item.ProductId));
							break;
						case DiscountApplication.Variants:
							voucherApplies = request.CartItems.Any(item =>
								voucher.ProductVariantIds != null &&
								voucher.ProductVariantIds.Contains(item.ProductVariantId));
							break;
					}

					if (!voucherApplies)
					{
						result.IsSuccess = false;
						result.Message = "Mã voucher không áp dụng cho sản phẩm trong giỏ hàng";
						result.FinalAmount = totalAmount - totalDiscount;
						return result;
					}

					// Calculate voucher discount
					decimal voucherDiscount = 0;

					if (voucher.ApplyTo == DiscountApplication.EntireOrder)
					{
						// Apply to entire cart
						voucherDiscount = (totalAmount - totalDiscount) * (voucher.DiscountPercentage / 100);
					}
					else
					{
						// Apply only to applicable items
						foreach (var item in request.CartItems)
						{
							bool itemApplies = false;
							switch (voucher.ApplyTo)
							{
								case DiscountApplication.Categories:
									itemApplies = item.CategoryId.HasValue &&
										voucher.CategoryIds != null &&
										voucher.CategoryIds.Contains(item.CategoryId.Value);
									break;
								case DiscountApplication.Products:
									itemApplies = voucher.ProductIds != null &&
										voucher.ProductIds.Contains(item.ProductId);
									break;
								case DiscountApplication.Variants:
									itemApplies = voucher.ProductVariantIds != null &&
										voucher.ProductVariantIds.Contains(item.ProductVariantId);
									break;
							}

							if (itemApplies)
							{
								var itemTotal = item.Price * item.Quantity;
								// Subtract automatic discount first
								var existingDiscount = result.ItemDiscounts.FirstOrDefault(d => d.ProductVariantId == item.ProductVariantId);
								if (existingDiscount != null)
								{
									itemTotal -= existingDiscount.DiscountAmount;
								}
								voucherDiscount += itemTotal * (voucher.DiscountPercentage / 100);
							}
						}
					}

					// Check maximum discount
					if (voucher.MaximumDiscountAmount.HasValue && voucherDiscount > voucher.MaximumDiscountAmount.Value)
					{
						voucherDiscount = voucher.MaximumDiscountAmount.Value;
					}

					totalDiscount += voucherDiscount;
					result.AppliedVoucher = MapToDto(voucher);
					result.Message = $"Áp dụng mã {request.VoucherCode} thành công! Giảm {voucherDiscount:N0}đ";
				}
				catch (Exception ex)
				{
					result.IsSuccess = false;
					result.Message = "Có lỗi khi áp dụng voucher: " + ex.Message;
				}
			}

			result.DiscountAmount = totalDiscount;
			result.FinalAmount = totalAmount - totalDiscount;

			return result;
		}

		#endregion

		#region Helper Methods

		// Map Sale entity to SaleDto
		private SaleDto MapToDto(Sale sale)
		{
			var now = DateTime.Now;
			var isWithinTimeRange = sale.StartDate <= now && now <= sale.EndDate;
			var hasUsageLeft = !sale.UsageLimit.HasValue || sale.UsedCount < sale.UsageLimit.Value;

			return new SaleDto
			{
				Id = sale.Id,
				Name = sale.Name,
				Description = sale.Description,
				DiscountType = sale.DiscountType,
				DiscountTypeName = GetDiscountTypeName(sale.DiscountType),
				VoucherCode = sale.VoucherCode,
				UsageLimit = sale.UsageLimit,
				UsedCount = sale.UsedCount,
				RemainingUsage = sale.UsageLimit.HasValue ? (sale.UsageLimit.Value - sale.UsedCount) : null,
				StartDate = sale.StartDate,
				EndDate = sale.EndDate,
				Status = GetSaleStatus(sale),
				DiscountPercentage = sale.DiscountPercentage,
				MaximumDiscountAmount = sale.MaximumDiscountAmount,
				MinimumOrderValue = sale.MinimumOrderValue,
				ApplyTo = sale.ApplyTo,
				ApplyToName = GetApplyToName(sale.ApplyTo),
				CategoryIds = sale.CategoryIds,
				ProductIds = sale.ProductIds,
				ProductVariantIds = sale.ProductVariantIds,
				IsActive = sale.IsActive,
				CanUse = sale.IsActive && isWithinTimeRange && hasUsageLeft,
				CreationTime = sale.CreationTime
			};
		}

		// Get status text
		private string GetSaleStatus(Sale sale)
		{
			var now = DateTime.Now;

			if (now < sale.StartDate)
			{
				return "Sắp diễn ra";
			}
			else if (now >= sale.StartDate && now <= sale.EndDate)
			{
				if (!sale.IsActive)
				{
					return "Đã tạm dừng";
				}
				if (sale.UsageLimit.HasValue && sale.UsedCount >= sale.UsageLimit.Value)
				{
					return "Đã hết lượt";
				}
				return "Đang hoạt động";
			}
			else
			{
				return "Đã kết thúc";
			}
		}

		// Get discount type display name
		private string GetDiscountTypeName(DiscountType type)
		{
			return type switch
			{
				DiscountType.Automatic => "Tự động",
				DiscountType.Voucher => "Voucher",
				_ => "Không xác định"
			};
		}

		// Get apply to display name
		private string GetApplyToName(DiscountApplication applyTo)
		{
			return applyTo switch
			{
				DiscountApplication.EntireOrder => "Toàn bộ đơn hàng",
				DiscountApplication.Categories => "Theo danh mục",
				DiscountApplication.Products => "Theo sản phẩm",
				DiscountApplication.Variants => "Theo biến thể",
				_ => "Không xác định"
			};
		}

		// Validate ApplyTo configuration
		private void ValidateApplyToConfiguration(DiscountApplication applyTo, List<int>? categoryIds, List<int>? productIds, List<int>? variantIds)
		{
			switch (applyTo)
			{
				case DiscountApplication.Categories:
					if (categoryIds == null || !categoryIds.Any())
					{
						throw new UserFriendlyException("Phải chọn ít nhất một danh mục khi áp dụng theo danh mục");
					}
					break;

				case DiscountApplication.Products:
					if (productIds == null || !productIds.Any())
					{
						throw new UserFriendlyException("Phải chọn ít nhất một sản phẩm khi áp dụng theo sản phẩm");
					}
					break;

				case DiscountApplication.Variants:
					if (variantIds == null || !variantIds.Any())
					{
						throw new UserFriendlyException("Phải chọn ít nhất một biến thể khi áp dụng theo biến thể");
					}
					break;

				case DiscountApplication.EntireOrder:
					// Không cần validate gì
					break;
			}
		}

		#endregion
	}
}
