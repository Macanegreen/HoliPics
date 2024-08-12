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

namespace HoliPics.Services.Implementations
{
    public class AzureEmailSenderService : IEmailSenderService
    {

        public AzureEmailSenderOptions Options { get; set; }

        public AzureEmailSenderService(IOptions<AzureEmailSenderOptions> options)
        {
            Options = options.Value;
        }

        public async Task<string> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return await ExecuteSendEmail(email, subject, htmlMessage);
        }

        public async Task<string> ExecuteSendEmail(string sendTo, string subject, string htmlMessage)
        {          
                        
            var emailClient = new EmailClient(Options.ConnectionString);
            string tmp = new TextPart(TextFormat.Html) { Text = htmlMessage }.ToString();
            Console.WriteLine(tmp);

            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Started,
                senderAddress: Options.SenderAddress,
                recipientAddress: sendTo,
                subject: subject,
                htmlContent: "<html><body>" + htmlMessage + "</body></html>",
                plainTextContent: htmlMessage);

            //try
            //{
            //    var emailSendOperation = emailClient.Send(
            //        wait: WaitUntil.Completed,
            //        senderAddress: "<Send email address>" // The email address of the domain registered with the Communication Services resource

            //        recipientAddress: "<recipient email address>"


            //        subject: "This is the subject",
            //        htmlContent: "<html><body>This is the html body</body></html>");
            //    Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

            //    /// Get the OperationId so that it can be used for tracking the message for troubleshooting
            //    string operationId = emailSendOperation.Id;
            //    Console.WriteLine($"Email operation id = {operationId}");
            //}
            //catch (RequestFailedException ex)
            //{
            //    /// OperationID is contained in the exception message and can be used for troubleshooting purposes
            //    Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            //}

            return "";
        }
    }
}
