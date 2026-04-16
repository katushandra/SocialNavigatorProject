using Application.Common.Interfaces;
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
    public class DeleteObjectResult
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public bool InvalidStatus { get; set; }
        public string? Error { get; set; }
    }

    public class DeleteObjectCommand : ICommand<DeleteObjectResult>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    public class DeleteObjectCommandHandler : IRequestHandler<DeleteObjectCommand, DeleteObjectResult>
    {
        private readonly ILocalDbContext context;
        private readonly ILogger<DeleteObjectCommandHandler> logger;

        public DeleteObjectCommandHandler(ILocalDbContext context, ILogger<DeleteObjectCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<DeleteObjectResult> Handle(DeleteObjectCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new DeleteObjectResult();

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == request.Id && x.CreatorId == request.UserId, cancellationToken);

            if (socialObject == null)
            {
                viewResult.NotFound = true;
                return viewResult;
            }

            if (socialObject.Status != Status.Pending)
            {
                viewResult.InvalidStatus = true;
                viewResult.Error = "Удалить можно только объекты со статусом 'На модерации'";
                return viewResult;
            }

            context.SocialObject.Remove(socialObject);
            await context.SaveChangesAsync(cancellationToken);

            viewResult.Success = true;

            logger.LogInformation("Пользователь {UserId} удалил объект {ObjectId}",
                request.UserId, request.Id);

            return viewResult;
        }
    }
}
