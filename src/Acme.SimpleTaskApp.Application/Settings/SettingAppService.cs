using Abp.Application.Services;
using Abp.Configuration;
using Abp.Runtime.Session;
using Abp.UI;
using Acme.SimpleTaskApp.Configuration;
using Acme.SimpleTaskApp.Settings.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Settings
{
	public class SettingAppService : ApplicationService, ISettingAppService
	{
		private readonly ISettingManager _settingManager;
		private readonly IAbpSession _abpSession;

		public SettingAppService(ISettingManager settingManager, IAbpSession abpSession)
		{
			_settingManager = settingManager;
			_abpSession = abpSession;
		}

		public async Task<GetAllSettingDto> GetAllSetting()
		{
			var setting = new GetAllSettingDto
			{
				MailSetting = await MailSettings()
			};
			return setting;
		}

		public async Task<MailSettingDto> MailSettings()
		{
			return new MailSettingDto
			{
				Server = await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.Server, 1),
				SmtpPort = int.Parse(await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.SmtpPort, 1)),
				SenderName = await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.SenderName, 1),
				EnableSsl = bool.Parse(await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.EnableSsl, 1)),
				SenderEmail = await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.SenderEmail, 1),
				UserName = await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.UserName, 1),
				Password = await _settingManager.GetSettingValueForTenantAsync(AppMailSettingNames.Password, 1)
			};
		}

		public async Task UpdateMailSettings(MailSettingDto input)
		{
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.Server, input.Server);
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.SmtpPort, input.SmtpPort.ToString());
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.SenderName, input.SenderName);
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.SenderEmail, input.SenderEmail);
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.UserName, input.UserName);
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.Password, input.Password);
			await _settingManager.ChangeSettingForApplicationAsync(AppMailSettingNames.EnableSsl, input.EnableSsl.ToString());
		}

		public async Task<TestSmtpResultDto> TestConnection(MailSettingDto input)
		{
			try
			{
				using (var client = CreateSmtpClient(input))
				{
					// Test connection by trying to connect
					await client.SendMailAsync(
							new MailMessage(input.SenderEmail, input.SenderEmail)
							{
								Subject = "SMTP Connection Test",
								Body = "This is a test email to verify SMTP connection.",
								IsBodyHtml = false
							}
					);

					return new TestSmtpResultDto
					{
						Success = true,
						Message = "SMTP connection test successful!"
					};
				}
			}
			catch (System.Exception ex)
			{
				return new TestSmtpResultDto
				{
					Success = false,
					Message = $"SMTP connection failed: {ex.Message}"
				};
			}
		}

		public async Task<TestSmtpResultDto> SendTestEmail(MailSettingDto input)
		{
			if (string.IsNullOrEmpty(input.TestEmailAddress))
			{
				throw new UserFriendlyException("Test email address is required.");
			}

			try
			{
				using (var client = CreateSmtpClient(input))
				{
					var mailMessage = new MailMessage
					{
						From = new MailAddress(input.SenderEmail, input.SenderName),
						Subject = "SMTP Configuration Test Email",
						Body = @$"
										<html>
										<body>
												<h2>SMTP Configuration Test</h2>
												<p>This is a test email to verify your SMTP settings.</p>
												<p><strong>Server:</strong> {input.Server}</p>
												<p><strong>Port:</strong> {input.SmtpPort}</p>
												<p><strong>SSL Enabled:</strong> {input.EnableSsl}</p>
												<p><strong>Time Sent:</strong> {System.DateTime.Now}</p>
												<hr>
												<p><em>If you received this email, your SMTP configuration is working correctly.</em></p>
										</body>
										</html>",
						IsBodyHtml = true
					};

					mailMessage.To.Add(input.TestEmailAddress);

					await client.SendMailAsync(mailMessage);

					return new TestSmtpResultDto
					{
						Success = true,
						Message = $"Test email sent successfully to {input.TestEmailAddress}"
					};
				}
			}
			catch (System.Exception ex)
			{
				return new TestSmtpResultDto
				{
					Success = false,
					Message = $"Failed to send test email: {ex.Message}"
				};
			}
		}

		private SmtpClient CreateSmtpClient(MailSettingDto settings)
		{
			return new SmtpClient(settings.Server, settings.SmtpPort)
			{
				EnableSsl = settings.EnableSsl,
				Credentials = new NetworkCredential(settings.UserName, settings.Password),
				UseDefaultCredentials = false
			};
		}
	}
}
