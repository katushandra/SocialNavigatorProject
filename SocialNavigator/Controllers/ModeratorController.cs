using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    [Authorize(Roles = "Moderator,Admin")]
    public class ModeratorController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly ILogger<ModeratorController> logger;

        public ModeratorController(ILocalDbContext context, IMapper mapper, UserManager<AppUser> userManager, IEmailService emailService, ILogger<ModeratorController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.emailService = emailService;
            this.logger = logger;
        }
        // метод для получения количества объектов на модерации
        private async Task<int> GetPending()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return 0;

            return await context.SocialObject
                .CountAsync(x => x.Status == Status.Pending);
        }

        #region ModeratorPanel Панель модерации
        [HttpGet]
        public async Task<IActionResult> ModeratorPanel (CancellationToken cancellationToken, string status)
        {
            var query = context.SocialObject
               .Include(x => x.ObjectType)
               .Include(x => x.Creator)
               .AsQueryable();

            var statusCounts = await query
               .GroupBy(x => x.Status)
               .Select(g => new { Status = g.Key, Count = g.Count() })
               .ToDictionaryAsync(x => x.Status.ToString(), x => x.Count, cancellationToken);

            if (string.IsNullOrEmpty(status))
            {
                status = "all";
            }

            if (status != "all")
            {
                bool isValid = Enum.GetNames(typeof(Status))
                                  .Any(name => string.Equals(name, status, StringComparison.OrdinalIgnoreCase));

                if (isValid)
                {
                    var statusEnum = (Status)Enum.Parse(typeof(Status), status, true);
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            var totalCount = statusCounts?.Sum(x => x.Value) ?? 0;
            var pendingCount = await GetPending();

            var moderationObjects = await query
                .ProjectTo<ModerationSocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            ViewBag.CurrentStatus = status;
            ViewBag.StatusCounts = statusCounts;
            ViewBag.TotalCount = totalCount;
            ViewBag.PendingCount = pendingCount;

            return View(moderationObjects);
        }
        #endregion

        #region ModeratorFull Информация об объекте
        [HttpGet]
        public async Task<IActionResult> ModeratorFull (Guid id, CancellationToken cancellationToken)
        {
            var socialObject = await context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Creator)
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(x => x.IdObject == id, cancellationToken);

            if (socialObject == null)
            {
                TempData["ErrorMessage"] = "Объект не найден";
                return RedirectToAction(nameof(ModeratorPanel));
            }

            var fullDto = mapper.Map<FullSocialObjectDto>(socialObject);

            return View(fullDto);
        }
        #endregion

        #region Approve Одобрение объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var socialObject = await context.SocialObject
                .Include(x => x.Creator)
                .FirstOrDefaultAsync(x => x.IdObject == id, cancellationToken);

            if (socialObject == null)
            {
                return NotFound();
            }

            var oldStatus = socialObject.Status;
            socialObject.Status = Status.Approved;
            socialObject.EditorId = user.Id;
            socialObject.EditedAt = DateTime.UtcNow;

            var moderationHistory = new ModerationHistory
            {
                ObjectId = socialObject.IdObject,
                ModeratorId = user.Id,
                OldStatus = oldStatus,
                NewStatus = Status.Approved,
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

            logger.LogInformation("Модератор {UserName} одобрил объект {ObjectId}", user.UserName, id);

            TempData["SuccessMessage"] = "Объект одобрен";
            return RedirectToAction(nameof(ModeratorPanel), new { status = "Pending" });
        }
        #endregion

        #region  Reject Отклонение объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string rejectionReason, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "Необходимо указать причину отклонения";
                return RedirectToAction(nameof(ModeratorFull), new { id });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var socialObject = await context.SocialObject
                .Include(x => x.Creator)
                .FirstOrDefaultAsync(x => x.IdObject == id, cancellationToken);

            if (socialObject == null)
            {
                return NotFound();
            }

            var oldStatus = socialObject.Status;
            socialObject.Status = Status.Rejected;
            socialObject.EditorId = user.Id;
            socialObject.EditedAt = DateTime.UtcNow;

            var moderationHistory = new ModerationHistory
            {
                ObjectId = socialObject.IdObject,
                ModeratorId = user.Id,
                OldStatus = oldStatus,
                NewStatus = Status.Rejected,
                Comment = rejectionReason,
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
                    {rejectionReason}
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

            logger.LogInformation("Модератор {UserName} отклонил объект {ObjectId} с причиной: {Reason}",
                user.UserName, id, rejectionReason);

            TempData["SuccessMessage"] = "Объект отклонен";
            return RedirectToAction(nameof(ModeratorPanel), new { status = "Pending" });
        }
        #endregion

        #region ModeratorHistory История модерации
        [HttpGet]
        public async Task<IActionResult> ModeratorHistory(CancellationToken cancellationToken)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var history = await context.ModerationHistory
                .Include(x => x.Object)
                .Include(x => x.Moderator)
                .Where(x => x.ModeratorId == currentUser.Id) 
                .OrderByDescending(x => x.ModeratedAt)
                .Take(10)
                .ProjectTo<ModerationHistoryDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            ViewBag.TotalCount = history.Count;

            return View(history);
        }
        #endregion
    }
}