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
		Task CreateOrUpdate(Products.ProductVariant input);
		Task Delete(int id);
		Task<Products.ProductVariant> GetById(int id);
		Task<PagedResultDto<Products.ProductVariant>> GetAll(GetProductVariantsInput input);
	}
}
