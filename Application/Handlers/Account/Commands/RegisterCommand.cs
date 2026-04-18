using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Account.Commands
{
    public class RegisterResult
    {
        public bool Succeeded { get; set; }
        public string? Email { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool ShowConfirmation { get; set; }
    }

    public class RegisterCommand : ICommand<RegisterResult>
    {
        public RegisterDto Model { get; set; } = null!;
        public Func<string, string, string, string>? UrlAction { get; set; }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly ILogger<RegisterCommandHandler> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;

        public RegisterCommandHandler(ILogger<RegisterCommandHandler> logger, UserManager<AppUser> userManager, IEmailService emailService)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.emailService = emailService;
        }

        public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new RegisterResult();

            var allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            if (string.IsNullOrWhiteSpace(request.Model.UserName))
            {
                viewResult.Errors.Add("Имя пользователя обязательно");
            }
            else
            {
                if (request.Model.UserName.Length < 3 || request.Model.UserName.Length > 50)
                {
                    viewResult.Errors.Add("Имя пользователя должно быть от 3 до 50 символов");
                }

                if (request.Model.UserName.Any(c => !allowedChars.Contains(c)))
                {
                    viewResult.Errors.Add("Имя пользователя может содержать только латинские буквы, цифры и символы -._@+");
                }
            }

            if (string.IsNullOrWhiteSpace(request.Model.Password))
            {
                viewResult.Errors.Add("Пароль обязателен");
            }
            else
            {
                var passwordErrors = PasswordValidator.Valid(request.Model.Password);
                if (passwordErrors.Any())
                {
                    viewResult.Errors.AddRange(passwordErrors);
                }
            }

            if (request.Model.Password != request.Model.PasswordConfirm)
            {
                viewResult.Errors.Add("Пароли не совпадают");
            }

            if (string.IsNullOrWhiteSpace(request.Model.Email))
            {
                viewResult.Errors.Add("Email обязателен");
            }
            else
            {
                var mailAddress = new System.Net.Mail.MailAddress(request.Model.Email);
                if (mailAddress.Address != request.Model.Email)
                {
                    viewResult.Errors.Add("Некорректный формат email");
                }
            }

            if (viewResult.Errors.Any())
            {
                return viewResult;
            }

            var existingUserByEmail = await userManager.FindByEmailAsync(request.Model.Email);
            if (existingUserByEmail != null)
            {
                viewResult.Errors.Add("Пользователь с таким email уже существует");
                return viewResult;
            }

            var existingUserByUserName = await userManager.FindByNameAsync(request.Model.UserName);
            if (existingUserByUserName != null)
            {
                viewResult.Errors.Add("Пользователь с таким именем уже существует");
                return viewResult;
            }


            var user = new AppUser
            {
                UserName = request.Model.UserName,
                Email = request.Model.Email,
                FullName = request.Model.FullName,
                UserCreatedAt = DateTime.UtcNow,
                Active = true,
            };

            var result = await userManager.CreateAsync(user, request.Model.Password);

            if (result.Succeeded)
            {
                var roleResult = await userManager.AddToRoleAsync(user, "User");

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = request.UrlAction?.Invoke(user.Id.ToString(), request.Model.Email, code);

                if (!string.IsNullOrEmpty(callbackUrl))
                {
                    await emailService.SendEmailAsync(request.Model.Email, "Подтверждение регистрации", $@"
                Здравствуйте, {user.FullName ?? user.UserName}!
                <br><br>
                Вы успешно зарегистрировались на сайте Социальный навигатор!<br>
                Для завершения регистрации перейдите по ссылке <a href='{callbackUrl}'> подтвердить email</a>
                <br><br>
                С уважением,<br>
                Команда Социальный навигатор!");
                }

                viewResult.Succeeded = true;
                viewResult.Email = request.Model.Email;
                viewResult.ShowConfirmation = true;
            }
            else
            {
                viewResult.Errors.AddRange(result.Errors.Select(e => e.Description));
            }

            return viewResult;
        }
    }
}