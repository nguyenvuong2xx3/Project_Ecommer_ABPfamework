using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Acme.SimpleTaskApp.Email.Dtos;
using Acme.SimpleTaskApp.Settings;
using System.Net;
using System.Net.Mail;

namespace Acme.SimpleTaskApp.Email
{
	/// Gửi email cho nhiều admin
	public class SendEmailToAdminsBackgroundJob : BackgroundJob<SendEmailToAdminsJobArgs>, ITransientDependency
	{
		private readonly ISettingAppService _settingAppService;

		public SendEmailToAdminsBackgroundJob(ISettingAppService settingAppService)
		{
			_settingAppService = settingAppService;
		}

		[UnitOfWork]
		public override void Execute(SendEmailToAdminsJobArgs args)
		{
			if (args.AdminEmails == null || args.AdminEmails.Count == 0)
			{
				Logger.Warn("Email admin không tồn tại");
				return;
			}

			var mailSettings = _settingAppService.MailSettings().Result;

			var smtpClient = new SmtpClient(mailSettings.Server, mailSettings.SmtpPort)
			{
				Credentials = new NetworkCredential(mailSettings.UserName, mailSettings.Password),
				EnableSsl = mailSettings.EnableSsl
			};

			foreach (var adminEmail in args.AdminEmails)
			{
				try
				{
					var mailMessage = new MailMessage
					{
						From = new MailAddress(mailSettings.UserName, mailSettings.SenderName),
						Subject = args.Subject,
						Body = args.Body,
						IsBodyHtml = true
					};

					mailMessage.To.Add(new MailAddress(adminEmail));

					smtpClient.Send(mailMessage);

					Logger.Info($"Email đã được gửi cho admin: {adminEmail}");
				}
				catch (System.Exception ex)
				{
					Logger.Error($"Lỗi gửi email admin: {adminEmail}", ex);
				}
			}
		}
	}
}
