using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Banners.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Banners
{
	public interface IBannerAppService: IApplicationService
	{
		Task<Banner> CreateBanner(CreateBannerDto input);
		Task<Banner> UpdateBanner(Banner input);
		Task DeleteBanner(int id);
		Task<PagedResultDto<Banner>> GetAllBanners(GetAllBannerDto input);
		Task<Banner> GetBannerById(int id);
	}
}
