using HoliPics.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using HoliPics.Services.Interfaces;

namespace HoliPics.Services.Implementations
{
    public class EmailSenderService : IEmailSenderService
    {

        public EmailSenderOptions Options { get; set; }

        public EmailSenderService(IOptions<EmailSenderOptions> options)
        {
            Options = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return ExecuteSendEmail(email, subject, htmlMessage);
        }

        public Task ExecuteSendEmail(string sendTo, string subject, string htmlMessage)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(Options.SenderEmail); 
            if (!string.IsNullOrEmpty(Options.SenderName))
            {
                email.Sender.Name = Options.SenderName;
            }
            email.From.Add(email.Sender);
            email.To.Add(MailboxAddress.Parse(sendTo));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage};

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(Options.HostAddress, Options.HostPort, Options.HostSecureSocketOptions);
                smtpClient.Authenticate(Options.HostUsername, Options.HostPassword);
                smtpClient.Send(email);
                smtpClient.Disconnect(true);
            }

            return Task.FromResult(true);
        }
    }
}
