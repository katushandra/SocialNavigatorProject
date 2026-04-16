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

namespace Application.Handlers.Object.Queries
{
    public class GetObjectTypesQuery : IQuery<List<ObjectTypeDto>>
    {
    }

    public class GetObjectTypesQueryHandler : IRequestHandler<GetObjectTypesQuery, List<ObjectTypeDto>>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetObjectTypesQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<List<ObjectTypeDto>> Handle(GetObjectTypesQuery request, CancellationToken cancellationToken)
        {
            return await context.ObjectType
                .OrderBy(x => x.Name)
                .ProjectTo<ObjectTypeDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}