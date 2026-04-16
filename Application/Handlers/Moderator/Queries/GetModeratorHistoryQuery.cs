using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Moderator.Queries
{
    public class GetModeratorHistoryResult
    {
        public List<ModerationHistoryDto> History { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GetModeratorHistoryQuery : IQuery<GetModeratorHistoryResult>
    {
        public Guid ModeratorId { get; set; }
    }

    public class GetModeratorHistoryQueryHandler : IRequestHandler<GetModeratorHistoryQuery, GetModeratorHistoryResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetModeratorHistoryQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<GetModeratorHistoryResult> Handle(GetModeratorHistoryQuery request, CancellationToken cancellationToken)
        {
            var result = new GetModeratorHistoryResult();

            result.History = await context.ModerationHistory
                .Include(x => x.Object)
                .Include(x => x.Moderator)
                .Where(x => x.ModeratorId == request.ModeratorId)
                .OrderByDescending(x => x.ModeratedAt)
                .Take(10)
                .ProjectTo<ModerationHistoryDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            result.TotalCount = result.History.Count;

            return result;
        }
    }
}