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

            var to = new { email = sendTo };
            var from = new { email = Options.From, name = "Mailtrap Test" };
            var args = new
            {
                from = from,
                to = new[] { to },
                subject = "You are awesome",
                text = "Congrats for sending your first email with Mailtrap!",
                category = "Integration Test"
            };           

            var client = new RestClient("https://send.api.mailtrap.io/api/send");
            var request = new RestRequest();
            request.AddHeader("Authorization", "Bearer " + Options.ApiToken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json",
            JsonSerializer.Serialize(args), ParameterType.RequestBody);
            var response = client.Post(request);
            Console.WriteLine(response.Content);


            return "";
        }
    }
}
