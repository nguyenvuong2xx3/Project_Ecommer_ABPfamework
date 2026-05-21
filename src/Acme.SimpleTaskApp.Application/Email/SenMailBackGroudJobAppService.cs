using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.Email.Dtos;
using Acme.SimpleTaskApp.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

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

            if (!TryBuildMailMessage(
                getAll.Result.UserName,
                getAll.Result.SenderName,
                targetUser.EmailAddress,
                targetUser.Name,
                args.Subject,
                args.Body,
                out var mailMessage,
                out var buildError))
            {
                throw new FormatException(buildError);
            }

            // mailMessage = new MailMessage
            //{
            //    From = new MailAddress(getAll.Result.UserName, getAll.Result.SenderName),
            //    Subject = args.Subject,
            //    Body = args.Body,
            //    IsBodyHtml = true,
            //    To = { new MailAddress(targetUser.EmailAddress, targetUser.Name) }
            //};

            smtpClient.Send(mailMessage);
        }

        private static bool TryNormalizeEmail(string raw, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var s = raw.Trim();

            // Nếu có dạng "Display Name <email@host>" -> lấy phần trong <...>
            var m = Regex.Match(s, @"<([^>]+)>");
            if (m.Success)
            {
                s = m.Groups[1].Value.Trim();
            }

            // Loại bỏ dấu ngoặc kép bao quanh nếu có
            s = s.Trim('\"');

            // Từ chối nhiều địa chỉ
            if (s.Contains(",") || s.Contains(";")) return false;

            // Sử dụng MailAddress.TryCreate để validate
            if (MailAddress.TryCreate(s, out _))
            {
                normalized = s;
                return true;
            }

            return false;
        }

        private static bool TryBuildMailMessage(
            string fromRaw,
            string fromDisplayName,
            string toRaw,
            string toDisplayName,
            string subject,
            string body,
            out MailMessage message,
            out string error)
        {
            message = null;
            error = null;

            if (!TryNormalizeEmail(fromRaw, out var fromEmail))
            {
                error = $"Invalid From address: '{fromRaw}'";
                return false;
            }

            if (!TryNormalizeEmail(toRaw, out var toEmail))
            {
                error = $"Invalid To address: '{toRaw}'";
                return false;
            }

            message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromDisplayName ?? string.Empty),
                Subject = subject ?? string.Empty,
                Body = body ?? string.Empty,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail, toDisplayName ?? string.Empty));
            return true;
        }
    }
}
