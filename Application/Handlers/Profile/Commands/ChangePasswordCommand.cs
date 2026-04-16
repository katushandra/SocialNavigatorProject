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
            var vewResult = new ChangePasswordResult();

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                vewResult.Succeeded = false;
                return vewResult;
            }

            var checkPassword = await userManager.CheckPasswordAsync(user, request.Model.CurrentPassword);
            if (!checkPassword)
            {
                vewResult.CurrentPasswordInvalid = true;
                vewResult.Errors.Add("Неверный текущий пароль");
                return vewResult;
            }

            if (request.Model.CurrentPassword == request.Model.NewPassword)
            {
                vewResult.SamePassword = true;
                vewResult.Errors.Add("Новый пароль должен отличаться от текущего");
                return vewResult;
            }

            var passwordErrors = PasswordValidator.Valid(request.Model.NewPassword);
            if (passwordErrors.Any())
            {
                vewResult.Errors.AddRange(passwordErrors);
                return vewResult;
            }

            var changeResult = await userManager.ChangePasswordAsync(user, request.Model.CurrentPassword, request.Model.NewPassword);

            if (changeResult.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                vewResult.Succeeded = true;

                logger.LogInformation("Пользователь {UserName} успешно сменил пароль", user.UserName);
                await signInManager.RefreshSignInAsync(user);
            }
            else
            {
                vewResult.Errors = changeResult.Errors.Select(e => e.Description).ToList();
            }

            return vewResult;
        }
    }
}
