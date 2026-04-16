using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Handlers.Account.Commands
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public bool IsLockedOut { get; set; }
        public string? Error { get; set; }
        public string? ReturnUrl { get; set; }
        public AppUser? User { get; set; }
    }

    public class LoginCommand : ICommand<LoginResult>
    {
        public LoginDto Model { get; set; } = null!;
        public string? ReturnUrl { get; set; }
    }   

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly ILogger<LoginCommandHandler> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public LoginCommandHandler(ILogger<LoginCommandHandler> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new LoginResult { ReturnUrl = request.ReturnUrl };

            var user = await userManager.FindByNameAsync(request.Model.UserName);

            if (user != null)
            {
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    viewResult.Error = "Вы не подтвердили свой email";
                    return viewResult;
                }
            }

            if (user == null)
            {
                viewResult.Error = "Неверное имя пользователя или пароль";
                return viewResult;
            }

            if (!user.Active)
            {
                viewResult.Error = "Ваш аккаунт деактивирован. Обратитесь к администратору";
                return viewResult;
            }

            var result = await signInManager.PasswordSignInAsync(user, request.Model.Password, false, lockoutOnFailure: true); //false - не запоминать, lockoutOnFailure: true - блокировать при ошибках

            if (result.Succeeded)
            {
                logger.LogInformation("Пользователь {UserName} успешно вошел в систему", request.Model.UserName);
                viewResult.Succeeded = true;
                viewResult.User = user;
            }
            else if (result.IsLockedOut)
            {
                logger.LogWarning("Аккаунт {UserName} заблокирован", request.Model.UserName);
                viewResult.IsLockedOut = true;
                viewResult.Error = "Аккаунт временно заблокирован. Попробуйте позже.";
            }
            else
            {
                viewResult.Error = "Неверное имя пользователя или пароль.";
            }
            return viewResult;
        }
    }
}