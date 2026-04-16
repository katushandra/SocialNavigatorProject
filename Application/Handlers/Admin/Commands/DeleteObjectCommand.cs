using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Admin.Commands
{
    public class DeleteObjectResult
    {
        public bool Success { get; set; }
        public bool NotFound { get; set; }
        public string? Error { get; set; }
    }
    public class DeleteObjectCommand : ICommand<DeleteObjectResult>
    {
        public Guid Id { get; set; }
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
            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == request.Id, cancellationToken);

            if (socialObject == null)
            {
                return new DeleteObjectResult { NotFound = true };
            }

            context.SocialObject.Remove(socialObject);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Администратор удалил объект {ObjectId}", request.Id);
            return new DeleteObjectResult { Success = true };
        }
    }
}