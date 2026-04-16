using Application.Common.Interfaces;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Account.Commands
{
    public class ConfirmEmailResult
    {
        public bool Succeeded { get; set; }
        public bool UserNotFound { get; set; }
    }

    public class ConfirmEmailCommand : ICommand<ConfirmEmailResult>
    {
        public string? UserId { get; set; }
        public string? Code { get; set; }
    }
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
    {
        private readonly UserManager<AppUser> userManager;

        public ConfirmEmailCommandHandler(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == null || request.Code == null)
            {
                return new ConfirmEmailResult { Succeeded = false };
            }

            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new ConfirmEmailResult { UserNotFound = true };
            }

            var result = await userManager.ConfirmEmailAsync(user, request.Code);
            return new ConfirmEmailResult { Succeeded = result.Succeeded };
        }
    }
}
