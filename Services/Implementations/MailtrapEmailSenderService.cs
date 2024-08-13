using HoliPics.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using HoliPics.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Azure;
using Azure.Communication.Email;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace HoliPics.Services.Implementations
{
    public class MailtrapEmailSenderService : IEmailSenderService
    {

        public MailtrapEmailSenderOptions Options { get; set; }

        public MailtrapEmailSenderService(IOptions<MailtrapEmailSenderOptions> options)
        {
            Options = options.Value;
        }

        public async Task<string> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return await ExecuteSendEmail(email, subject, htmlMessage);
        }

        public async Task<string> ExecuteSendEmail(string sendTo, string subject, string htmlMessage)
        {

            //var to = new { email = sendTo };
            //var from = new { email = Options.From, name = "Mailtrap Test" };
            //var args = new
            //{
            //    from = from,
            //    to = new[] { to },
            //    subject = "You are awesome",
            //    text = "Congrats for sending your first email with Mailtrap!",
            //    category = "Integration Test"
            //};           

            //var client = new RestClient("https://send.api.mailtrap.io/api/send");
            //var request = new RestRequest();
            //request.AddHeader("Authorization", "Bearer " + Options.ApiToken);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("application/json",
            //JsonSerializer.Serialize(args), ParameterType.RequestBody);
            //var response = client.Post(request);
            //Console.WriteLine(response.Content);

            //var email = new MimeMessage();
            //email.Sender = MailboxAddress.Parse(Options.);
            //if (!string.IsNullOrEmpty(Options.SenderName))
            //{
            //    email.Sender.Name = Options.SenderName;
            //}
            //email.From.Add(email.Sender);
            //email.To.Add(MailboxAddress.Parse(sendTo));
            //email.Subject = subject;
            //email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            //using (var smtpClient = new SmtpClient())
            //{
            //    await smtpClient.ConnectAsync(Options.HostAddress, Options.HostPort, Options.HostSecureSocketOptions);
            //    await smtpClient.AuthenticateAsync(Options.HostUsername, Options.HostPassword);
            //    await smtpClient.SendAsync(email);
            //    await smtpClient.DisconnectAsync(true);
            //}


            var message = new System.Net.Mail.MailMessage(Options.From, sendTo);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = htmlMessage;
            var smtp = new System.Net.Mail.SmtpClient("live.smtp.mailtrap.io", 587);
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential("api", Options.ApiToken);
            smtp.Send(message);

            return "";
        }
    }
}
