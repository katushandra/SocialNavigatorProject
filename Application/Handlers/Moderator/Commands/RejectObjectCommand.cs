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
    public class RejectObjectResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public bool NoReason { get; set; }
        public string? Error { get; set; }
    }

    public class RejectObjectCommand : ICommand<RejectObjectResult>
    {
        public Guid Id { get; set; }
        public Guid ModeratorId { get; set; }
        public string RejectionReason { get; set; } = null!;
    }

    public class RejectObjectCommandHandler : IRequestHandler<RejectObjectCommand, RejectObjectResult>
    {
        private readonly ILocalDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly ILogger<RejectObjectCommandHandler> logger;

        public RejectObjectCommandHandler(
            ILocalDbContext context,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ILogger<RejectObjectCommandHandler> logger)
        {
            this.context = context;
            this.userManager = userManager;
            this.emailService = emailService;
            this.logger = logger;
        }

        public async Task<RejectObjectResult> Handle(RejectObjectCommand request, CancellationToken cancellationToken)
        {
            var result = new RejectObjectResult();

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                result.NoReason = true;
                result.Error = "Необходимо указать причину отклонения";
                return result;
            }

            var moderator = await userManager.FindByIdAsync(request.ModeratorId.ToString());
            if (moderator == null)
            {
                result.Error = "Модератор не найден";
                return result;
            }

            var socialObject = await context.SocialObject
                .Include(x => x.Creator)
                .FirstOrDefaultAsync(x => x.IdObject == request.Id, cancellationToken);

            if (socialObject == null)
            {
                result.NotFound = true;
                return result;
            }

            var oldStatus = socialObject.Status;
            socialObject.Status = Status.Rejected;
            socialObject.EditorId = request.ModeratorId;
            socialObject.EditedAt = DateTime.UtcNow;

            var moderationHistory = new ModerationHistory
            {
                ObjectId = socialObject.IdObject,
                ModeratorId = request.ModeratorId,
                OldStatus = oldStatus,
                NewStatus = Status.Rejected,
                Comment = request.RejectionReason,
                ModeratedAt = DateTime.UtcNow
            };

            context.ModerationHistory.Add(moderationHistory);
            await context.SaveChangesAsync(cancellationToken);

            if (socialObject.Creator != null)
            {
                try
                {
                    await emailService.SendEmailAsync(socialObject.Creator.Email, "Объект отклонен", $@"
                    Здравствуйте, {socialObject.Creator.FullName ?? socialObject.Creator.UserName}!
                    <br><br>
                    Ваш объект <strong>{socialObject.Name}</strong> не прошел модерацию.
                    <br><br>
                    <strong>Причина отклонения:</strong><br>
                    {request.RejectionReason}
                    <br><br>
                    Вы можете отредактировать объект и отправить его на повторную проверку.
                    <br><br>
                    С уважением,<br>
                    Команда Социальный навигатор!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при отправке email автору {Email}", socialObject.Creator.Email);
                }
            }

            logger.LogInformation("Модератор {ModeratorName} отклонил объект {ObjectId} с причиной: {Reason}",
                moderator.UserName, request.Id, request.RejectionReason);

            result.Succeeded = true;
            return result;
        }
    }
}
