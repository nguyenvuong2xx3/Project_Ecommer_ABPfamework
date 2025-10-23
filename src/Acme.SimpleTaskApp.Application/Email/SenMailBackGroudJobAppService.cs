using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Email.Dtos;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Acme.SimpleTaskApp.Email
{
	public class SenMailBackGroudJobAppService : BackgroundJob<SendEmailJobArgs>, ITransientDependency
	{
		private readonly IRepository<User, long> _userRepository;
		private readonly IEmailSender _emailSender;
		private readonly IConfiguration _configuration;

		public SenMailBackGroudJobAppService(
				IRepository<User, long> userRepository,
				IEmailSender emailSender,
				IConfiguration configuration)
		{
			_userRepository = userRepository;
			_emailSender = emailSender;
			_configuration = configuration;
		}

		[UnitOfWork]
		public override void Execute(SendEmailJobArgs args)
		{
			var senderUser = _userRepository.Get(args.SenderUserId);
			var targetUser = _userRepository.Get(args.TargetUserId);

			var smtpHost = _configuration["EmailSettings:SmtpHost"];
			var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
			var smtpUser = _configuration["EmailSettings:SmtpUser"];
			var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
			var smtpEnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

			var smtpClient = new SmtpClient(smtpHost, smtpPort)
			{
				Credentials = new NetworkCredential(smtpUser, smtpPassword),
				EnableSsl = smtpEnableSsl
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(smtpUser, "Quản trị viên"),
				Subject = args.Subject,
				Body = args.Body,
				IsBodyHtml = true,
				To = { new MailAddress(targetUser.EmailAddress, targetUser.Name) }
			};

			smtpClient.Send(mailMessage);
		}
	}
}
