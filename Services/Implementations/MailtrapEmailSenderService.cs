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
