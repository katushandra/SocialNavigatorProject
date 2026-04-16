using Application.Common.Interfaces;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Account.Commands
{
    public class ForgotPasswordResult
    {
        public bool Success { get; set; } = true;
    }

    public class ForgotPasswordCommand : ICommand<ForgotPasswordResult>
    {
        public ForgotPasswordDto Model { get; set; } = null!;
        public Func<string, string, string, string>? UrlAction { get; set; }
    }
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> logger;

        public ForgotPasswordCommandHandler(UserManager<AppUser> userManager, IEmailService emailService, ILogger<ForgotPasswordCommandHandler> logger)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new ForgotPasswordResult();
            var user = await userManager.FindByEmailAsync(request.Model.Email);

            if (user == null)
            {
                logger.LogInformation("Попытка сбросить пароль. Пользователь с email {Email} не найден.", request.Model.Email);
                return viewResult;
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                logger.LogInformation("Попытка сбросить пароль. {Email} не подтвержден, инструкция не отправлена.", request.Model.Email);
                return viewResult;
            }
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = request.UrlAction?.Invoke(user.Id.ToString(), request.Model.Email, code);

            await emailService.SendEmailAsync(request.Model.Email, "Сброс пароля", $@"
                Здравствуйте, {user.FullName ?? user.UserName}!
                <br><br>
                Для сброса пароля перейдите по ссылке <a href='{callbackUrl}'>cбросить пароль</a>
                <br><br>
                Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.
                <br><br>
                С уважением,<br>
                Команда Социальный навигатор!");

            logger.LogInformation("Отправлена инструкция по сбросу пароля на email {Email}", request.Model.Email);
            return viewResult;
        }
    }
}