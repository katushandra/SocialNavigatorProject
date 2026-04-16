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
    public class LogoutCommand : ICommand
    {
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
    {
        private readonly SignInManager<AppUser> signInManager;

        public LogoutCommandHandler(SignInManager<AppUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            await signInManager.SignOutAsync();
        }
    }
}
