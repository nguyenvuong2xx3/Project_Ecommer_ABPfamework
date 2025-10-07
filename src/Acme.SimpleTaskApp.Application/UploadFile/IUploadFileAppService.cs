using Abp.Application.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.UploadFile
{
	public interface IUploadFileAppService : IApplicationService
	{
		string UploadImageAsync(IFormFile file, string subFolder = "products");
		Task RemoveImage(string imageUrl);
	}
}
