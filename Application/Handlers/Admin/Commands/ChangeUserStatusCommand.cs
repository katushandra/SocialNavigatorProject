using Application.Common.Interfaces;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Admin.Commands
{
    public class ChangeUserStatusResult
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public bool NewActiveStatus { get; set; }
        public string? Error { get; set; }
    }
    public class ChangeUserStatusCommand : ICommand<ChangeUserStatusResult>
    {
        public Guid UserId { get; set; }
    }
    public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, ChangeUserStatusResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ChangeUserStatusCommandHandler> logger;

        public ChangeUserStatusCommandHandler(UserManager<AppUser> userManager, ILogger<ChangeUserStatusCommandHandler> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<ChangeUserStatusResult> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return new ChangeUserStatusResult { NotFound = true };
            }

            user.Active = !user.Active;
            user.UserEditedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("Администратор изменил статус пользователя {UserName} на {Active}", user.UserName, user.Active);

                if (!user.Active)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                }

                return new ChangeUserStatusResult { Success = true, NewActiveStatus = user.Active };
            }

            return new ChangeUserStatusResult { Success = false, Error= "Ошибка при изменении статуса" };
        }
    }
}