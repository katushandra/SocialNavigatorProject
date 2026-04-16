using Application.Common.Interfaces;
using Domain.DTO.Admin;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Admin.Queries
{
    public class GetAdminPanelQuery : IQuery<AdminPanelDto>
    {
    }

    public class GetAdminPanelQueryHandler : IRequestHandler<GetAdminPanelQuery, AdminPanelDto>
    {
        private readonly ILocalDbContext context;
        private readonly UserManager<AppUser> userManager;

        public GetAdminPanelQueryHandler(ILocalDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<AdminPanelDto> Handle(GetAdminPanelQuery request, CancellationToken cancellationToken)
        {
            var totalObjects = await context.SocialObject.CountAsync(cancellationToken);

            var totalApprovedObjects = await context.SocialObject
                .CountAsync(x => x.Status == Status.Approved, cancellationToken);

            var totalUsers = await userManager.Users.CountAsync(cancellationToken);

            var averageRating = await context.Review
                .AverageAsync(x => (double?)x.Score, cancellationToken) ?? 0;

            var objectsByType = await context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.Status == Status.Approved)
                .GroupBy(x => x.ObjectType.Name)
                .Select(g => new TypeStatisticDto
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var type in objectsByType)
            {
                if (totalApprovedObjects > 0)
                {
                    type.Percentage = Math.Round((double)type.Count / totalApprovedObjects * 100, 1);
                }
                else
                {
                    type.Percentage = 0;
                }
            }

            var topUsers = await context.SocialObject
              .Include(x => x.Creator)
              .Where(x => x.Creator != null)
              .GroupBy(x => new { x.Creator.Id, x.Creator.UserName, x.Creator.FullName })
              .Select(g => new UserActivityDto
              {
                  UserId = g.Key.Id,
                  UserName = g.Key.UserName,
                  FullName = g.Key.FullName,
                  ObjectsCount = g.Count(),
                  ReviewsCount = context.Review.Count(r => r.UserId == g.Key.Id)
              })
              .OrderByDescending(u => u.ObjectsCount)
              .Take(10)
              .ToListAsync(cancellationToken);

            return new AdminPanelDto
            {
                TotalObjects = totalObjects,
                TotalApprovedObjects = totalApprovedObjects,
                TotalUsers = totalUsers,
                AverageRating = Math.Round((decimal)averageRating, 1),
                ObjectsByType = objectsByType,
                TopActiveUsers = topUsers
            };
        }
    }
}