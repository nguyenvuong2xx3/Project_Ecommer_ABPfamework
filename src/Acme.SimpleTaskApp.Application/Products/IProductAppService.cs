﻿using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Acme.SimpleTaskApp.Products.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Acme.SimpleTaskApp.Products
{
	public interface IProductAppService : IApplicationService
	{
		Task<PagedResultDto<ProductListDto>> GetAllProducts(GetAllProductsInput input);

		Task<ProductListDto> CreateProducts(CreateProductDto input);

		Task<ProductListDto> GetByIdProducts(EntityDto<int> input);

		Task DeleteProducts(EntityDto<int> input);

		Task<ProductListDto> UpdateProducts(UpdateProductDto input);

		Task<PagedResultDto<ProductListDto>> SearchProducts(GetAllProductsInput input);

		//Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file);

		Task<byte[]> ExportProductsToExcel(GetAllProductsInput input);

		Task<List<ImportProductResultDto>> ImportProductsFromExcel(IFormFile file);


	}
}
