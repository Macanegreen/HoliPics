using HoliPics.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HoliPics.Services.Implementations
{
    public class EmailSenderService : IEmailSenderService
    {

        public EmailSenderOptions Options { get; set; }

        public EmailSenderService(IOptions<EmailSenderOptions> options)
        {
            Options = options.Value;
        }

        public async Task<string> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return await ExecuteSendEmail(email, subject, htmlMessage);
        }

        public async Task<string> ExecuteSendEmail(string sendTo, string subject, string htmlMessage)
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
                await smtpClient.ConnectAsync(Options.HostAddress, Options.HostPort, Options.HostSecureSocketOptions);
                await smtpClient.AuthenticateAsync(Options.HostUsername, Options.HostPassword);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }

            return "";
        }
    }
}
