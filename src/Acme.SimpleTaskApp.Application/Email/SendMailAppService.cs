using Abp.Application.Services;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Services;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Configuration;
using Acme.SimpleTaskApp.Email.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Email
{
	public class SendMailAppService : ApplicationService, IDomainService, ISendMailAppService
	{
		private readonly IBackgroundJobManager _backgroundJobManager;
		private readonly IEmailSender _emailSender;
		private readonly UserManager _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ISettingManager _settingManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public SendMailAppService(
			IEmailSender emailSender, 
			UserManager userManager, 
			IWebHostEnvironment webHostEnvironment, 
			IBackgroundJobManager backgroundJobManager,
			ISettingManager settingManager,
			IHttpContextAccessor httpContextAccessor)
		{
			_webHostEnvironment = webHostEnvironment;
			_emailSender = emailSender;
			_userManager = userManager;
			_backgroundJobManager = backgroundJobManager;
			_settingManager = settingManager;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task SendMailOrderAsync()
		{
			var currentUserId = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
			DateTime currentTime = DateTime.Now;
			string formattedTime = currentTime.ToString("dd/MM/yyyy HH:mm:ss");

			// Get store settings
			var storeName = await _settingManager.GetSettingValueAsync(AppNameStore.NameStore);
			var logoUrl = await _settingManager.GetSettingValueAsync(AppNameStore.UrlLogo);

			// Fallback if not configured
			if (string.IsNullOrWhiteSpace(storeName))
			{
				storeName = "Cửa hàng của bạn";
			}

			// Convert relative URL to absolute URL for email
			if (!string.IsNullOrWhiteSpace(logoUrl) && !logoUrl.StartsWith("http"))
			{
				var request = _httpContextAccessor.HttpContext?.Request;
				if (request != null)
				{
					var baseUrl = $"{request.Scheme}://{request.Host}";
					logoUrl = baseUrl + logoUrl;
				}
			}

			// Đọc nội dung file HTML (đường dẫn tương đối hoặc tuyệt đối)
			var webRootPath = _webHostEnvironment.WebRootPath;

			string htmlTemplatePath = Path.Combine(webRootPath, "assets", "EmailTemplates", "EmailDatHangThanhCong.html");

			string htmlBody = await File.ReadAllTextAsync(htmlTemplatePath);

			// Thay các placeholder trong template (bao gồm store settings)
			htmlBody = htmlBody
					.Replace("{{UserName}}", currentUserId.Name ?? "Khách hàng")
					.Replace("{{OrderTime}}", formattedTime)
					.Replace("{{StoreName}}", storeName)
					.Replace("{{LogoUrl}}", logoUrl ?? "");

			await _backgroundJobManager.EnqueueAsync<SenMailBackGroudJobAppService, SendEmailJobArgs>(
							new SendEmailJobArgs
							{
								Subject = $"{storeName} - Xác nhận đơn hàng",
								Body = htmlBody,
								SenderUserId = AbpSession.GetUserId(),
								TargetUserId = currentUserId.Id
							});
		}
	}
}
