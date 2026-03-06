using Application.Common.Interfaces;
using MailKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Email
{
    public class EmailService: IEmailService
    {
        private readonly ILogger<EmailService> logger; 
        private readonly Smtp smtp;

        public EmailService(ILogger<EmailService> logger, IOptions<Smtp> smtp)
        {
            this.logger = logger;
            this.smtp = smtp.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            /*
            logger.LogInformation("ПОДТВЕРЖДЕНИЕ EMAIL");
            logger.LogInformation("Кому: {Email}", email);
            logger.LogInformation("Тема: {Subject}", subject);
            logger.LogInformation("Сообщение: {Message}", message);

            await Task.CompletedTask;
            */
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(smtp.FromName, smtp.FromEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtp.Host, smtp.Port, smtp.EnableSsl);
                await client.AuthenticateAsync(smtp.UserName, smtp.Password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }

            logger.LogInformation("Письмо отправлено на адрес {Email}", email);
        }
    }
}