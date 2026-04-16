using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers.Admin.Queries
{
    public class AdminModerationHistoryResult
    {
        public List<ModerationHistoryDto> History { get; set; } = new();
        public string SearchTerm { get; set; } = "";
    }
    public class GetAdminModerationHistoryQuery : IQuery<AdminModerationHistoryResult>
    {
        public string SearchTerm { get; set; } = "";
    }

    public class GetAdminModerationHistoryQueryHandler : IRequestHandler<GetAdminModerationHistoryQuery, AdminModerationHistoryResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetAdminModerationHistoryQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<AdminModerationHistoryResult> Handle(GetAdminModerationHistoryQuery request, CancellationToken cancellationToken)
        {
            var query = context.ModerationHistory
                .Include(x => x.Object)
                .Include(x => x.Moderator)
                .OrderByDescending(x => x.ModeratedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(x =>
                    x.Object.Name.ToLower().Contains(searchTerm) ||
                    (x.Comment != null && x.Comment.ToLower().Contains(searchTerm)));
            }

            var history = await query
                .ProjectTo<ModerationHistoryDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new AdminModerationHistoryResult
            {
                History = history,
                SearchTerm = request.SearchTerm
            };
        }
    }
}