using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        public EmailService(ILogger<EmailService> logger)
        {
            this.logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            logger.LogInformation("ПОДТВЕРЖДЕНИЕ EMAIL");
            logger.LogInformation("Кому: {Email}", email);
            logger.LogInformation("Тема: {Subject}", subject);
            logger.LogInformation("Сообщение: {Message}", message);

            await Task.CompletedTask;
        }
    }
}
