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

namespace Application.Handlers.Object.Queries
{
    public class GetFullQuery : IQuery<FullSocialObjectDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetObjectFullQueryHandler : IRequestHandler<GetFullQuery, FullSocialObjectDto?>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetObjectFullQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<FullSocialObjectDto?> Handle(GetFullQuery request, CancellationToken cancellationToken)
        {
            return await context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User)
                .Where(x => x.IdObject == request.Id && x.Status == Status.Approved)
                .ProjectTo<FullSocialObjectDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
