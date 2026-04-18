using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Profile.Commands
{
    public class ChangePasswordResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool CurrentPasswordInvalid { get; set; }
        public bool SamePassword { get; set; }
    }

    public class ChangePasswordCommand : ICommand<ChangePasswordResult>
    {
        public ChangePasswordDto Model { get; set; } = null!;
        public Guid UserId { get; set; }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<ChangePasswordCommandHandler> logger;

        public ChangePasswordCommandHandler(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new ChangePasswordResult();

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                viewResult.Succeeded = false;
                return viewResult;
            }

            if (string.IsNullOrWhiteSpace(request.Model.CurrentPassword))
            {
                viewResult.CurrentPasswordInvalid = true;
                viewResult.Errors.Add("Текущий пароль обязателен");
            }
            else
            {
                var checkPassword = await userManager.CheckPasswordAsync(user, request.Model.CurrentPassword);
                if (!checkPassword)
                {
                    viewResult.CurrentPasswordInvalid = true;
                    viewResult.Errors.Add("Неверный текущий пароль");
                }
            }

            if (string.IsNullOrWhiteSpace(request.Model.NewPassword))
            {
                viewResult.Errors.Add("Новый пароль обязателен");
            }
            else
            {
                var passwordErrors = PasswordValidator.Valid(request.Model.NewPassword);
                if (passwordErrors.Any())
                {
                    viewResult.Errors.AddRange(passwordErrors);
                }

                if (request.Model.CurrentPassword == request.Model.NewPassword)
                {
                    viewResult.SamePassword = true;
                    viewResult.Errors.Add("Новый пароль должен отличаться от текущего");
                }               
            }

            if (request.Model.NewPassword != request.Model.ConfirmPassword)
            {
                viewResult.Errors.Add("Пароли не совпадают");
            }

            if (viewResult.Errors.Any())
            {
                return viewResult;
            }

            var changeResult = await userManager.ChangePasswordAsync(user, request.Model.CurrentPassword, request.Model.NewPassword);

            if (changeResult.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                viewResult.Succeeded = true;

                logger.LogInformation("Пользователь {UserName} успешно сменил пароль", user.UserName);
                await signInManager.RefreshSignInAsync(user);
            }
            else
            {
                viewResult.Errors = changeResult.Errors.Select(e => e.Description).ToList();
            }

            return viewResult;
        }
    }
}
