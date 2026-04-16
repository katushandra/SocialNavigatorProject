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

namespace Application.Handlers.Admin.Queries
{
    public class AdminAllObjectsResult
    {
        public List<ModerationSocialObjectDto> Objects { get; set; } = new();
        public List<string> ObjectTypes { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class GetAdminAllObjectsQuery : IQuery<AdminAllObjectsResult>
    {
        public string SearchTerm { get; set; } = "";
        public string Status { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class GetAdminAllObjectsQueryHandler : IRequestHandler<GetAdminAllObjectsQuery, AdminAllObjectsResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetAdminAllObjectsQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<AdminAllObjectsResult> Handle(GetAdminAllObjectsQuery request, CancellationToken cancellationToken)
        {
            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Creator)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                request.SearchTerm = request.SearchTerm.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(request.SearchTerm) ||
                    x.Address.ToLower().Contains(request.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "all")
            {
                if (Enum.TryParse<Status>(request.Status, true, out var statusEnum))
                {
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                query = query.Where(x => x.ObjectType.Name == request.Type);
            }

            var objects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<ModerationSocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var objectTypes = await context.ObjectType
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            return new AdminAllObjectsResult
            {
                SearchTerm = request.SearchTerm,
                Status = request.Status,
                Type = request.Type,                
                ObjectTypes = objectTypes,
                Objects = objects,
            };
        }
    }
}