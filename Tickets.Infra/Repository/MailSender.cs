using FleetLinker.Domain.Entity;
using FleetLinker.Domain.IRepository;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
namespace FleetLinker.Infra.Repository
{
    public class MailSender : IEmailSender
    {
        public MailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
        private AuthMessageSenderOptions Options { get; } //set only via Secret Manager
        public Task SendEmailAsync(string host, int? port, bool ssl, string emailSender, string password, string email, string subject, string message, string fromName, string fromEmail)
        {
            return Execute(host, port, ssl, emailSender, password, subject, message, email, fromName, fromEmail);
        }
        private async Task Execute(string host, int? port, bool ssl, string emailSender, string password, string subject, string message, string emails, string fromName, string fromEmail)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(emailSender);
                if (!string.IsNullOrEmpty(fromName))
                    email.Sender.Name = fromName;
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                if (!string.IsNullOrEmpty(emails))
                {
                    foreach (var mail in emails.Split(";"))
                    {
                        email.To.Add(MailboxAddress.Parse(mail));
                    }
                }
                email.To.Add(MailboxAddress.Parse(emails));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = message };
                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(host, (int)port, SecureSocketOptions.SslOnConnect);
                    await smtp.AuthenticateAsync(emailSender, password);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
            }
            catch(Exception ex)
            {}
        }
    }
}
