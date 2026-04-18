using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Account.Commands
{
    public class ResetPasswordResult
    {
        public bool Succeeded { get; set; }
        public bool UserNotFound { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ResetPasswordCommand : ICommand<ResetPasswordResult>
    {
        public ResetPasswordDto Model { get; set; } = null!;
    }
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ResetPasswordCommandHandler> logger;

        public ResetPasswordCommandHandler(UserManager<AppUser> userManager, ILogger<ResetPasswordCommandHandler> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new ResetPasswordResult();

            if (string.IsNullOrWhiteSpace(request.Model.Password))
            {
                viewResult.Errors.Add("Новый пароль обязателен");
            }
            else
            {
                var passwordErrors = PasswordValidator.Valid(request.Model.Password);
                if (passwordErrors.Any())
                {
                    viewResult.Errors.AddRange(passwordErrors);
                }
            }

            if (request.Model.Password != request.Model.ConfirmPassword)
            {
                viewResult.Errors.Add("Пароли не совпадают");
            }

            if (viewResult.Errors.Any())
            {
                return viewResult;
            }

            var user = await userManager.FindByEmailAsync(request.Model.Email);
            if (user == null)
            {
                return viewResult;
            }

            var result= await userManager.ResetPasswordAsync(user, request.Model.Code, request.Model.Password);

            if (result.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
                logger.LogInformation("Пользователь {UserName} успешно сбросил пароль", user.UserName);
                viewResult.Succeeded = true;
                return viewResult;
            }

            viewResult.Errors.AddRange(result.Errors.Select(e => e.Description));
            return viewResult;
        }
    }
}