using Abp.Configuration.Startup;
using Abp.MailKit;
using Abp.Modules;
using Abp.Net.Mail.Smtp;
using MailKit.Net.Smtp;

namespace Acme.SimpleTaskApp.Email
{
	public class MyMailKitSmtpBuilder : DefaultMailKitSmtpBuilder
	{
		public MyMailKitSmtpBuilder(ISmtpEmailSenderConfiguration smtpEmailSenderConfiguration, IAbpMailKitConfiguration abpMailKitConfiguration)
				: base(smtpEmailSenderConfiguration, abpMailKitConfiguration)
		{
		}

		protected override void ConfigureClient(SmtpClient client)
		{
			client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

			base.ConfigureClient(client);
		}
	}

	[DependsOn(typeof(AbpMailKitModule))]
	public class MyProjectModule : AbpModule
	{
		public override void PreInitialize()
		{
			Configuration.ReplaceService<IMailKitSmtpBuilder, MyMailKitSmtpBuilder>();
		}
	}
}
