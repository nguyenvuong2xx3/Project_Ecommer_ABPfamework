using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Sales.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Sales
{
	public interface ISaleAppService : IApplicationService
	{
		// CRUD Operations
		Task<SaleDto> CreateSale(CreateSaleDto input);
		Task<SaleDto> UpdateSale(UpdateSaleDto input);
		Task DeleteSale(int id);
		Task<SaleDto> GetSaleById(int id);
		Task<PagedResultDto<SaleDto>> GetAllSales(GetAllSaleDto input);

		// Voucher Operations
		Task<SaleDto> GetSaleByVoucherCode(string voucherCode);
		Task<bool> ValidateVoucherCode(string voucherCode, decimal orderValue);
		Task IncrementUsedCount(int saleId);

		// Check Operations
		Task<bool> IsSaleActive(int id);
		Task<bool> IsSaleApplicableForOrder(int saleId, decimal orderValue);
		Task<List<SaleDto>> GetActiveSalesForProduct(int productId);
		Task<List<SaleDto>> GetActiveSalesForCategory(int categoryId);
		Task<List<SaleDto>> GetActiveSalesForVariant(int variantId);

		// Auto Sales (không cần mã)
		Task<List<SaleDto>> GetActiveAutomaticSales();

		// Statistics
		Task<Dictionary<string, object>> GetSaleStatistics(int saleId);

		// NEW: Calculate Discount Methods
		Task<decimal> CalculateProductVariantDiscount(int productVariantId, int productId, int? categoryId);
		Task<DiscountCalculationResultDto> CalculateCartDiscount(RequestDisCountVoucherCart request);
		Task<SaleDto> GetBestSaleForProductVariant(int productVariantId, int productId, int? categoryId);
	}

	// New DTOs for discount calculation
	public class CartItemDiscountDto
	{
		public int ProductVariantId { get; set; }
		public int ProductId { get; set; }
		public int? CategoryId { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
	}

	public class RequestDisCountVoucherCart
	{
		public List<CartItemDiscountDto> CartItems { get; set; }
		public string VoucherCode { get; set; }
	}

	public class DiscountCalculationResultDto
	{
		public decimal TotalAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal FinalAmount { get; set; }
		public List<ItemDiscountDetailDto> ItemDiscounts { get; set; }
		public SaleDto AppliedVoucher { get; set; }
		public string Message { get; set; }
		public bool IsSuccess { get; set; }
	}

	public class ItemDiscountDetailDto
	{
		public int ProductVariantId { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal DiscountPercentage { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal FinalPrice { get; set; }
		public SaleDto AppliedSale { get; set; }
	}
}
