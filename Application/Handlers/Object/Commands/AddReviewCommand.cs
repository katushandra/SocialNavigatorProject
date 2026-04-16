using Application.Common.Helpers;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Object.Commands
{
    public class AddReviewResult
    {
        public bool Succeeded { get; set; }
        public bool ObjectNotFound { get; set; }
        public string? Error { get; set; }
    }

    public class AddReviewCommand : ICommand<AddReviewResult>
    {
        public AddReviewDto Model { get; set; } = null!;
        public Guid UserId { get; set; }
    }

    public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, AddReviewResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AddReviewCommandHandler> logger;

        public AddReviewCommandHandler(ILocalDbContext context, IMapper mapper, ILogger<AddReviewCommandHandler> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<AddReviewResult> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new AddReviewResult();

            var objectExists = await context.SocialObject
                .AnyAsync(x => x.IdObject == request.Model.ObjectId && x.Status == Status.Approved, cancellationToken);

            if (!objectExists)
            {
                viewResult.ObjectNotFound = true;
                return viewResult;
            }

            var review = mapper.Map<Review>(request.Model);
            review.UserId = request.UserId;

            context.Review.Add(review);
            await context.SaveChangesAsync(cancellationToken);

            await UpdateObjectScore.UpdateScore(context, request.Model.ObjectId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            viewResult.Succeeded = true;

            logger.LogInformation("Пользователь {UserId} добавил отзыв на объект {ObjectId}",
                request.UserId, request.Model.ObjectId);

            return viewResult;
        }

    }
}
