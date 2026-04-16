using Application.Common.Interfaces;
using AutoMapper;
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
    public class GetModeratorFullObjectQuery : IQuery<FullSocialObjectDto?>
    {
        public Guid Id { get; set; }
    }

    public class GetModeratorFullObjectQueryHandler : IRequestHandler<GetModeratorFullObjectQuery, FullSocialObjectDto?>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetModeratorFullObjectQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<FullSocialObjectDto?> Handle(GetModeratorFullObjectQuery request, CancellationToken cancellationToken)
        {
            var socialObject = await context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Creator)
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(x => x.IdObject == request.Id, cancellationToken);

            if (socialObject == null)
                return null;

            return mapper.Map<FullSocialObjectDto>(socialObject);
        }
    }
}