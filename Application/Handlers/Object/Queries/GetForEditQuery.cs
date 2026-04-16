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

namespace Application.Handlers.Object.Queries
{
    public class GetForEditQuery : IQuery<AddSocialObjectDto?>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    public class GetObjectForEditQueryHandler : IRequestHandler<GetForEditQuery, AddSocialObjectDto?>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetObjectForEditQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<AddSocialObjectDto?> Handle(GetForEditQuery request, CancellationToken cancellationToken)
        {
            var socialObject = await context.SocialObject
                .Include(x => x.ObjectType)
                .FirstOrDefaultAsync(x => x.IdObject == request.Id && x.CreatorId == request.UserId, cancellationToken);

            if (socialObject == null)
                return null;

            return mapper.Map<AddSocialObjectDto>(socialObject);
        }
    }
}
