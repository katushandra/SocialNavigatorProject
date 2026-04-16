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

namespace Application.Handlers.Home.Queries
{
    public class GetHomePageResult
    {
        public List<SocialObjectDto> SocialObjects { get; set; } = new();
        public List<object> MapObjects { get; set; } = new();
        public List<string> ObjectTypes { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string ObjectTypeFilter { get; set; } = "";
        public string AccessibilityFilter { get; set; } = "";
    }

    public class GetHomePageQuery : IQuery<GetHomePageResult>
    {
        public string? SearchTerm { get; set; }
        public string? ObjectType { get; set; }
        public string? Accessibility { get; set; }
    }

    public class GetHomePageQueryHandler : IRequestHandler<GetHomePageQuery, GetHomePageResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetHomePageQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<GetHomePageResult> Handle(GetHomePageQuery request, CancellationToken cancellationToken)
        {
            var result = new GetHomePageResult
            {
                SearchTerm = request.SearchTerm ?? "",
                ObjectTypeFilter = request.ObjectType ?? "",
                AccessibilityFilter = request.Accessibility ?? ""
            };

            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.Status == Status.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.Address.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.ObjectType))
            {
                query = query.Where(x => x.ObjectType.Name == request.ObjectType);
            }

            if (!string.IsNullOrWhiteSpace(request.Accessibility))
            {
                switch (request.Accessibility)
                {
                    case "wheelchairs":
                        query = query.Where(x => x.Wheelchairs);
                        break;
                    case "blind":
                        query = query.Where(x => x.BlindAccess);
                        break;
                    case "deaf":
                        query = query.Where(x => x.DeafAccess);
                        break;
                    case "speech":
                        query = query.Where(x => x.SpeechAccess);
                        break;
                    case "mobility":
                        query = query.Where(x => x.MobilityAccess);
                        break;
                    case "intellectual":
                        query = query.Where(x => x.IntellectualAccess);
                        break;
                    case "autism":
                        query = query.Where(x => x.AutismAccess);
                        break;
                }
            }

            result.SocialObjects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<SocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            result.MapObjects = result.SocialObjects
                .Where(x => x.Location != null)
                .Select(x => new
                {
                    x.IdObject,
                    x.Name,
                    x.Address,
                    x.ScoreObject,
                    Latitude = x.Location!.Y,
                    Longitude = x.Location!.X,
                    ObjectTypeName = x.ObjectType?.NameObjectType,
                    Accessibility = new
                    {
                        x.Wheelchairs,
                        x.BlindAccess,
                        x.DeafAccess,
                        x.SpeechAccess,
                        x.MobilityAccess,
                        x.IntellectualAccess,
                        x.AutismAccess
                    }
                })
                .ToList<object>();

            result.ObjectTypes = await context.ObjectType
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}