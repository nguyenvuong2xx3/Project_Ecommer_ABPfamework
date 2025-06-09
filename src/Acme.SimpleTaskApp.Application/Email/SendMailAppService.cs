using Abp.Application.Services;
using Abp.Domain.Services;
using Abp.Net.Mail;
using Acme.SimpleTaskApp.Authorization.Users;
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
		private readonly IEmailSender _emailSender;
		private readonly UserManager _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public SendMailAppService(IEmailSender emailSender, UserManager userManager, IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
			_emailSender = emailSender;
			_userManager = userManager;
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

			var smtpClient = new SmtpClient("smtp.gmail.com", 587)
			{
				Credentials = new NetworkCredential("vuongmot2k3@gmail.com", "pxky aeqn qclp cauy"),
				EnableSsl = true
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress("vuongmot2k3@gmail.com", "Quản trị viên"),
				Subject = "Hệ thống bán hàng HUMG",
				Body = htmlBody,
				IsBodyHtml = true,
			};

			mailMessage.To.Add(currentUserId.EmailAddress);

			await smtpClient.SendMailAsync(mailMessage);
		}
	}
}
