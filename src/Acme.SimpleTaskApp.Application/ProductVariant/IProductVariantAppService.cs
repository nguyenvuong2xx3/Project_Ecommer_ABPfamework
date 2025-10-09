using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Acme.SimpleTaskApp.ProductVariant.Dtos;
using System;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.ProductVariant
{
	public interface IProductVariantAppService : IApplicationService
	{
		Task CreateProductVariant(Products.ProductVariant input);
		Task EditProductVariant(Products.ProductVariant input);
		Task DeleteProductVariant(int id);
		Task<PagedResultDto<Products.ProductVariant>> GetAllProductVariant(GetProductVariantsInput input);
		Task<Products.ProductVariant> GetById(int id);
	}
}
