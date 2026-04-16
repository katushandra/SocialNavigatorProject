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

namespace Application.Handlers.Profile.Queries
{
    public class GetMyReviewsResult
    {
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class GetMyReviewsQuery : IQuery<GetMyReviewsResult>
    {
        public Guid UserId { get; set; }
    }

    public class GetMyReviewsQueryHandler : IRequestHandler<GetMyReviewsQuery, GetMyReviewsResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public GetMyReviewsQueryHandler(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<GetMyReviewsResult> Handle(GetMyReviewsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetMyReviewsResult();

            result.Reviews = await context.Review
                .Include(x => x.Object)
                .Where(x => x.UserId == request.UserId)
                .OrderByDescending(x => x.ReviewCreatedAt)
                .ProjectTo<ReviewDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}