using Abp.Application.Services;
using Abp.BackgroundJobs;
using Abp.Domain.Services;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Email.Dtos;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Email
{
	public class SendMailAppService : ApplicationService, IDomainService
	{
		private readonly IBackgroundJobManager _backgroundJobManager;
		private readonly IEmailSender _emailSender;
		private readonly UserManager _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public SendMailAppService(IEmailSender emailSender, UserManager userManager, IWebHostEnvironment webHostEnvironment, IBackgroundJobManager backgroundJobManager)
		{
			_webHostEnvironment = webHostEnvironment;
			_emailSender = emailSender;
			_userManager = userManager;
			_backgroundJobManager = backgroundJobManager;
		}

		public async Task SendMailOrderAsync()
		{
			var currentUserId = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
			DateTime currentTime = DateTime.Now;
			string formattedTime = currentTime.ToString("dd/MM/yyyy HH:mm:ss");

			// Đọc nội dung file HTML (đường dẫn tương đối hoặc tuyệt đối)
			var webRootPath = _webHostEnvironment.WebRootPath;

			string htmlTemplatePath = webRootPath + Path.DirectorySeparatorChar.ToString() + "assets" + Path.DirectorySeparatorChar.ToString() + "EmailTemplates" + Path.DirectorySeparatorChar.ToString() + "EmailDatHangThanhCong.html";

			//string htmlTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "order_mail_template.html");
			string htmlBody = await File.ReadAllTextAsync(htmlTemplatePath);

			// Thay các placeholder trong template (nếu có)
			htmlBody = htmlBody
					.Replace("{{UserName}}", currentUserId.Name)
					.Replace("{{OrderTime}}", formattedTime);

			await _backgroundJobManager.EnqueueAsync<SenMailBackGroudJobAppService, SendEmailJobArgs>(
							new SendEmailJobArgs
							{
								Subject = "Hệ thống bán hàng HUMG",
								Body = htmlBody,
								SenderUserId = AbpSession.GetUserId(),
								TargetUserId = currentUserId.Id
							});
		}
	}
}
