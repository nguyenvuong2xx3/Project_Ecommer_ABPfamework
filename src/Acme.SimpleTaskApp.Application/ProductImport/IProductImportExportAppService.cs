using Abp.Application.Services;
using Acme.SimpleTaskApp.ProductImport.Dtos;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductImport
{
	public interface IProductImportExportAppService : IApplicationService
	{
		Task<ExportResult> ExportProducts(ExportProductInput input);
		Task<ImportProductRowResult> ProcessProductRowAsync(ExcelWorksheet worksheet, int row);
		Task<ImportProductRowResult> ProcessVariantRowAsync(ExcelWorksheet worksheet, int row, int productId);
	}
}
