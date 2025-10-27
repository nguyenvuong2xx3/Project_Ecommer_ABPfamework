using Abp.Application.Services;
using Acme.SimpleTaskApp.Locations.Dtos;
using Acme.SimpleTaskApp.Orders;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public interface ILocationAppService : IApplicationService
{
	Task<List<TinhThanhDto>> GetAllDonViHanhChinh();
}

public class LocationAppService : ApplicationService, ILocationAppService 
{
	private readonly string _filePath;

	public LocationAppService(IWebHostEnvironment env)
	{
		_filePath = Path.Combine(env.ContentRootPath, "danhmucxaphuong.json");
	}

	public async Task<List<TinhThanhDto>> GetAllDonViHanhChinh()
	{
		if (!File.Exists(_filePath))
			throw new FileNotFoundException("Không tìm thấy file dữ liệu địa phương!");

		var json = await File.ReadAllTextAsync(_filePath);

		var settings = new JsonSerializerSettings
		{
			MissingMemberHandling = MissingMemberHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore
		};

		var data = JsonConvert.DeserializeObject<List<TinhThanhDto>>(json, settings)
								?? new List<TinhThanhDto>();

		return data;
	}
}
