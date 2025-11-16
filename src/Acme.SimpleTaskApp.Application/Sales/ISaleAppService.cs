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

		// Auto Sales (không c?n mã)
		Task<List<SaleDto>> GetActiveAutomaticSales();

		// Statistics
		Task<Dictionary<string, object>> GetSaleStatistics(int saleId);
	}
}
