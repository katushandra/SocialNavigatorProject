using Application.Common.Helpers;
using Application.Common.Interfaces;
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
    public class DeleteReviewResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
    }

    public class DeleteReviewCommand : ICommand<DeleteReviewResult>
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
    }

    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, DeleteReviewResult>
    {
        private readonly ILocalDbContext context;
        private readonly ILogger<DeleteReviewCommandHandler> logger;

        public DeleteReviewCommandHandler(ILocalDbContext context, ILogger<DeleteReviewCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<DeleteReviewResult> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new DeleteReviewResult();

            var review = await context.Review
                .FirstOrDefaultAsync(x => x.IdReview == request.Id && x.UserId == request.UserId, cancellationToken);

            if (review == null)
            {
                viewResult.NotFound = true;
                return viewResult;
            }

            context.Review.Remove(review);

            await UpdateObjectScore.UpdateScore(context, request.ObjectId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            viewResult.Succeeded = true;

            logger.LogInformation("Пользователь {UserId} удалил отзыв {ReviewId}",
                request.UserId, request.Id);

            return viewResult;
        }
    }
}
