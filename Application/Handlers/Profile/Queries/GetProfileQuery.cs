using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Profile.Queries
{
    public class GetProfileResult
    {
        public AppUserDto? Profile { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public int? PendingCount { get; set; }
    }

    public class GetProfileQuery : IQuery<GetProfileResult?>
    {
        public Guid UserId { get; set; }
    }

    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileResult?>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;
        private readonly ILocalDbContext context;

        public GetProfileQueryHandler(UserManager<AppUser> userManager, IMapper mapper, ILocalDbContext context)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<GetProfileResult?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return null;

            var result = new GetProfileResult();
            result.Profile = mapper.Map<AppUserDto>(user);
            result.Roles = await userManager.GetRolesAsync(user);
            result.Profile.Role = result.Roles.FirstOrDefault() ?? "User";

            if (result.Roles.Contains("Moderator") || result.Roles.Contains("Admin"))
            {
                result.PendingCount = await context.SocialObject
                    .CountAsync(x => x.Status == Status.Pending, cancellationToken);
            }

            return result;
        }
    }
}