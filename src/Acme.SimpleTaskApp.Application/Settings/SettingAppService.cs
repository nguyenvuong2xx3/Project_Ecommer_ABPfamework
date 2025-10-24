using Abp.Configuration;
using Abp.Runtime.Session;
using Acme.SimpleTaskApp.Configuration;
using Acme.SimpleTaskApp.Settings.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Settings
{
	public class SettingAppService
	{
		private readonly ISettingManager _settingManager;
		private readonly IAbpSession _abpSession;

		public SettingAppService(ISettingManager settingManager, IAbpSession abpSession)
		{
			_settingManager = settingManager ?? throw new ArgumentNullException(nameof(settingManager));
			_abpSession = abpSession ?? throw new ArgumentNullException(nameof(abpSession));
		}

		private async Task<MailSettingDto> MailSettings()
		{
			return new MailSettingDto
			{
				Server = await _settingManager.GetSettingValueAsync(AppMailSettingNames.Server),
				SmtpPort = int.Parse(await _settingManager.GetSettingValueAsync(AppMailSettingNames.SmtpPort)),
				SenderName = await _settingManager.GetSettingValueAsync(AppMailSettingNames.SenderName),
				EnableSsl = bool.Parse(await _settingManager.GetSettingValueAsync(AppMailSettingNames.EnableSsl)),
				SenderEmail = await _settingManager.GetSettingValueAsync(AppMailSettingNames.SenderEmail),
				UserName = await _settingManager.GetSettingValueAsync(AppMailSettingNames.UserName),
				Password = await _settingManager.GetSettingValueAsync(AppMailSettingNames.Password)
			};
		}
		public async Task<MailSettingDto> GetMailSetting()
		{
			int? tenantId = _abpSession.GetTenantId();
			if (tenantId.HasValue)
			{
				throw new InvalidOperationException("Tenant ID is not available in the current session.");
			}
			return await MailSettings();
		}
		/// <summary>
		/// Updates the mail settings for the current tenant.
		/// </summary>
		/// <param name="input">The mail settings to update.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		public async Task UpdateMailSetting(MailSettingDto input)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.Server,
					input.Server);

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.SmtpPort,
					input.SmtpPort.ToString());

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.SenderName,
					input.SenderName);

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.EnableSsl,
					input.EnableSsl.ToString());

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.SenderEmail,
					input.SenderEmail);

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.UserName,
					input.UserName);

			await _settingManager.ChangeSettingForTenantAsync(
					_abpSession.GetTenantId(),
					AppMailSettingNames.Password,
					input.Password);
		}
	}
}
