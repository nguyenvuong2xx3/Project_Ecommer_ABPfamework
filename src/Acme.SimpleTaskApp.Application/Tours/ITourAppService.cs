using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Acme.SimpleTaskApp.Tours.Dtos;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Tours
{
	public interface ITourAppService : IApplicationService
	{
		Task<PagedResultDto<TourListDto>> GetAllTour(GetAllTourInput input);

		Task<TourListDto> CreateTour(CreateTourInput input);

		Task<TourListDto> GetByIdTour(EntityDto<long> input);

		Task DeleteTour(EntityDto<long> input);

		Task<TourListDto> UpdateTour(UpdateTourInput input);

		Task<PagedResultDto<TourListDto>> SearchTour(GetAllTourInput input);

	}
}
