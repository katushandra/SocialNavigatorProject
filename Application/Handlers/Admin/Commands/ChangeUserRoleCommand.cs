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
    public class ChangeUserRoleResult
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public string? Error{ get; set; }
    }
    public class ChangeUserRoleCommand : ICommand<ChangeUserRoleResult>
    {
        public Guid UserId { get; set; }
        public string NewRole { get; set; } = null!;
    }
    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ChangeUserRoleCommandHandler> logger;

        public ChangeUserRoleCommandHandler(UserManager<AppUser> userManager, ILogger<ChangeUserRoleCommandHandler> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<ChangeUserRoleResult> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return new ChangeUserRoleResult { NotFound = true };
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await userManager.AddToRoleAsync(user, request.NewRole);

            if (result.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                logger.LogInformation("Администратор изменил роль пользователя {UserName} на {Role}", user.UserName, request.NewRole);
                return new ChangeUserRoleResult { Success = true };
            }

            return new ChangeUserRoleResult { Success = false, Error = "Ошибка при изменении роли" };
        }
    }
}