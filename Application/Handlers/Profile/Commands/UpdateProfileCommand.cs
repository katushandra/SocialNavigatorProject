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

namespace Application.Handlers.Profile.Commands
{
    public class UpdateProfileResult
    {
        public bool Succeeded { get; set; }
        public bool EmailChanged { get; set; }
        public string? ConfirmationCode { get; set; }
        public string? CallbackUrl { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool EmailAlreadyExists { get; set; }
        public bool NoChanges { get; set; }
    }

    public class UpdateProfileCommand : ICommand<UpdateProfileResult>
    {
        public AppUserDto Model { get; set; } = null!;
        public Guid UserId { get; set; }
        public Func<string, string, string, string?>? UrlAction { get; set; }
    }

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly ILogger<UpdateProfileCommandHandler> logger;
        private readonly SignInManager<AppUser> signInManager;

        public UpdateProfileCommandHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ILogger<UpdateProfileCommandHandler> logger,
            SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.logger = logger;
            this.signInManager = signInManager;
        }

        public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new UpdateProfileResult();

            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                viewResult.Succeeded = false;
                return viewResult;
            }

            bool hasChange = false;
            bool emailChange = false;

            if (user.FullName != request.Model.FullName)
            {
                user.FullName = request.Model.FullName;
                hasChange = true;
            }

            if (user.Email != request.Model.Email)
            {
                var existingUserByEmail = await userManager.FindByEmailAsync(request.Model.Email);
                if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                {
                    viewResult.EmailAlreadyExists = true;
                    viewResult.Errors.Add("Пользователь с таким email уже существует");
                    return viewResult;
                }
                user.Email = request.Model.Email;
                user.NormalizedEmail = request.Model.Email.ToUpperInvariant();
                user.EmailConfirmed = false;
                emailChange = true;
                hasChange = true;
            }

            if (!hasChange)
            {
                viewResult.NoChanges = true;
                return viewResult;
            }
            else
            {
                user.UserEditedAt = DateTime.UtcNow;
            }                           

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                viewResult.Succeeded = false;
                viewResult.Errors = result.Errors.Select(e => e.Description).ToList();
                return viewResult;
            }

            if (emailChange)
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = request.UrlAction?.Invoke(user.Id.ToString(), request.Model.Email, code);

                await emailService.SendEmailAsync(request.Model.Email, "Подтверждение смены email", $@"
                    Здравствуйте, {user.FullName ?? user.UserName}!
                    <br><br>
                    Для подтверждения нового email перейдите по ссылке <a href='{callbackUrl}'> подтвердить email</a>
                    <br><br>
                    С уважением,<br>
                    Команда Социальный навигатор!");

                viewResult.EmailChanged = true;
                viewResult.ConfirmationCode = code;
                viewResult.CallbackUrl = callbackUrl;
            }

            viewResult.Succeeded = true;

            logger.LogInformation("Пользователь {UserName} обновил профиль", user.UserName);
            await signInManager.RefreshSignInAsync(user);

            return viewResult;
        }
    }
}