using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Email.Dtos;
using Acme.SimpleTaskApp.Settings;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Acme.SimpleTaskApp.Email
{
	public class SenMailBackGroudJobAppService : BackgroundJob<SendEmailJobArgs>, ITransientDependency
	{
		private readonly IRepository<User, long> _userRepository;
		private readonly IEmailSender _emailSender;
		private readonly ISettingAppService _settingAppService;

		public SenMailBackGroudJobAppService(
				IRepository<User, long> userRepository,
				IEmailSender emailSender,
				ISettingAppService settingAppService)
		{
			_settingAppService = settingAppService;
			_userRepository = userRepository;
			_emailSender = emailSender;
		}

		[UnitOfWork]
		public override void Execute(SendEmailJobArgs args)
		{
			var senderUser = _userRepository.Get(args.SenderUserId);
			var targetUser = _userRepository.Get(args.TargetUserId);

			var getAll = _settingAppService.MailSettings();
			var smtpClient = new SmtpClient(getAll.Result.Server, getAll.Result.SmtpPort)
			{
				Credentials = new NetworkCredential(getAll.Result.UserName, getAll.Result.Password),
				EnableSsl = getAll.Result.EnableSsl
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(getAll.Result.UserName, getAll.Result.SenderName),
				Subject = args.Subject,
				Body = args.Body,
				IsBodyHtml = true,
				To = { new MailAddress(targetUser.EmailAddress, targetUser.Name) }
			};

			smtpClient.Send(mailMessage);
		}
	}
}
