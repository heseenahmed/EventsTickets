using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
namespace Tickets.Infra.Repository
{
    public class MailRepository :IMailRepository
    {
        private readonly MailSettings _mailSettings;
        private readonly IAppLocalizer _localizer;
        public MailRepository(IOptions<MailSettings> mailSettings , IAppLocalizer localizer)
        {
            _mailSettings = mailSettings.Value;
            _localizer = localizer;
        }
        public async Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile> attachments = null)
        {
            var email = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_mailSettings.Email),
                Subject = subject
            };
            if (!MailboxAddress.TryParse(mailTo, out var toAddress))
                throw new Exception(_localizer[LocalizationMessages.InvalidEmailAddress]);
            email.To.Add(toAddress);
            var builder = new BodyBuilder();
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = body;
            email.Body = builder.ToMessageBody();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.Auto);
            smtp.Authenticate(_mailSettings.Email, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
