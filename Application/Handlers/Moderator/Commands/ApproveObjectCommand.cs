using Application.Common.Interfaces;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Moderator.Commands
{
    public class ApproveObjectResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public string? Error { get; set; }
    }

    public class ApproveObjectCommand : ICommand<ApproveObjectResult>
    {
        public Guid Id { get; set; }
        public Guid ModeratorId { get; set; }
    }

    public class ApproveObjectCommandHandler : IRequestHandler<ApproveObjectCommand, ApproveObjectResult>
    {
        private readonly ILocalDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly ILogger<ApproveObjectCommandHandler> logger;

        public ApproveObjectCommandHandler(
            ILocalDbContext context,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ILogger<ApproveObjectCommandHandler> logger)
        {
            this.context = context;
            this.userManager = userManager;
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task<ApproveObjectResult> Handle(ApproveObjectCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new ApproveObjectResult();

            var moderator = await userManager.FindByIdAsync(request.ModeratorId.ToString());
            if (moderator == null)
            {
                viewResult.Error = "Модератор не найден";
                return viewResult;
            }

            var socialObject = await context.SocialObject
                .Include(x => x.Creator)
                .FirstOrDefaultAsync(x => x.IdObject == request.Id, cancellationToken);

            if (socialObject == null)
            {
                viewResult.NotFound = true;
                return viewResult;
            }

            var oldStatus = socialObject.Status;
            socialObject.Status = Status.Approved;
            socialObject.EditorId = request.ModeratorId;
            socialObject.EditedAt = DateTime.UtcNow;

            var moderationHistory = new ModerationHistory
            {
                ObjectId = socialObject.IdObject,
                ModeratorId = request.ModeratorId,
                OldStatus = oldStatus,
                NewStatus = Status.Approved,
                Comment = "Объект одобрен",
                ModeratedAt = DateTime.UtcNow
            };

            context.ModerationHistory.Add(moderationHistory);
            await context.SaveChangesAsync(cancellationToken);

            if (socialObject.Creator != null)
            {
                try
                {
                    await emailService.SendEmailAsync(socialObject.Creator.Email, "Объект одобрен", $@"
                    Здравствуйте, {socialObject.Creator.FullName ?? socialObject.Creator.UserName}!
                    <br><br>
                    Ваш объект <strong>{socialObject.Name}</strong> прошел модерацию и опубликован на карте.
                    <br><br>
                    С уважением,<br>
                    Команда Социальный навигатор!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при отправке email автору {Email}", socialObject.Creator.Email);
                }
            }

            logger.LogInformation("Модератор {ModeratorName} одобрил объект {ObjectId}", moderator.UserName, request.Id);

            viewResult.Succeeded = true;
            return viewResult;
        }
    }
}