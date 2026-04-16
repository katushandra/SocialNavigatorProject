using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Moderator.Queries
{
    public class GetModeratorPanelResult
    {
        public List<ModerationSocialObjectDto> Objects { get; set; } = new();
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public string CurrentStatus { get; set; } = "all";
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class GetModeratorPanelQuery : IQuery<GetModeratorPanelResult>
    {
        public string? Status { get; set; }
    }

    public class GetModeratorPanelQueryHandler : IRequestHandler<GetModeratorPanelQuery, GetModeratorPanelResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetModeratorPanelQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<GetModeratorPanelResult> Handle(GetModeratorPanelQuery request, CancellationToken cancellationToken)
        {
            var result = new GetModeratorPanelResult();

            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Creator)
                .AsQueryable();

            var statusCounts = await query
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count, cancellationToken);

            result.StatusCounts = statusCounts;

            var status = string.IsNullOrEmpty(request.Status) ? "all" : request.Status;
            result.CurrentStatus = status;

            if (status != "all")
            {
                bool isValid = Enum.GetNames(typeof(Status))
                    .Any(name => string.Equals(name, status, StringComparison.OrdinalIgnoreCase));

                if (isValid)
                {
                    var statusEnum = (Status)Enum.Parse(typeof(Status), status, true);
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            result.Objects = await query
                .ProjectTo<ModerationSocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            result.TotalCount = statusCounts?.Sum(x => x.Value) ?? 0;
            result.PendingCount = statusCounts?.GetValueOrDefault("Pending", 0) ?? 0;

            return result;
        }
    }
}