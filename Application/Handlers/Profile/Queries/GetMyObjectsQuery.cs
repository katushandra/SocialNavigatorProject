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

namespace Application.Handlers.Profile.Queries
{
    public class GetMyObjectsResult
    {
        public List<MyObjectDto> Objects { get; set; } = new();
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public string CurrentStatus { get; set; } = "all";
        public int TotalCount { get; set; }
    }

    public class GetMyObjectsQuery : IQuery<GetMyObjectsResult>
    {
        public Guid UserId { get; set; }
        public string? Status { get; set; }
    }

    public class GetMyObjectsQueryHandler : IRequestHandler<GetMyObjectsQuery, GetMyObjectsResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetMyObjectsQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<GetMyObjectsResult> Handle(GetMyObjectsQuery request, CancellationToken cancellationToken)
        {
            var viewResult = new GetMyObjectsResult();

            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.CreatorId == request.UserId);

            var statusCounts = await query
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count, cancellationToken);

            viewResult.StatusCounts = statusCounts;

            var status = string.IsNullOrEmpty(request.Status) ? "all" : request.Status;
            viewResult.CurrentStatus = status;

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

            viewResult.Objects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<MyObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            viewResult.TotalCount = viewResult.Objects.Count;

            return viewResult;
        }
    }
}