using Application.Handlers.Moderator.Commands;
using Application.Handlers.Moderator.Queries;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    [Authorize(Roles = "Moderator,Admin")]
    public class ModeratorController : Controller
    {
        private readonly IMediator mediator;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ModeratorController> logger;

        public ModeratorController(IMediator mediator, UserManager<AppUser> userManager, ILogger<ModeratorController> logger)
        {
            this.mediator = mediator;
            this.userManager = userManager;
            this.logger = logger;
        }
        /* метод для получения количества объектов на модерации
        private async Task<int> GetPending()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return 0;

            return await context.SocialObject
                .CountAsync(x => x.Status == Status.Pending);
        }
        */
        #region ModeratorPanel Панель модерации
        [HttpGet]
        public async Task<IActionResult> ModeratorPanel(CancellationToken cancellationToken, string status)
        {
            var result = await mediator.Send(new GetModeratorPanelQuery
            {
                Status = status
            }, cancellationToken);

            ViewBag.CurrentStatus = result.CurrentStatus;
            ViewBag.StatusCounts = result.StatusCounts;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.PendingCount = result.PendingCount;

            return View(result.Objects);
        }
        #endregion

        #region ModeratorFull Информация об объекте
        [HttpGet]
        public async Task<IActionResult> ModeratorFull(Guid id, CancellationToken cancellationToken)
        {
            var fullDto = await mediator.Send(new GetModeratorFullObjectQuery
            {
                Id = id
            }, cancellationToken);

            if (fullDto == null)
            {
                TempData["ErrorMessage"] = "Объект не найден";
                return RedirectToAction(nameof(ModeratorPanel));
            }

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

            var result = await mediator.Send(new ApproveObjectCommand
            {
                Id = id,
                ModeratorId = user.Id
            }, cancellationToken);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Объект одобрен";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при одобрении объекта";
            }

            return RedirectToAction(nameof(ModeratorPanel), new { status = "Pending" });
        }
        #endregion

        #region  Reject Отклонение объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string rejectionReason, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await mediator.Send(new RejectObjectCommand
            {
                Id = id,
                ModeratorId = user.Id,
                RejectionReason = rejectionReason
            }, cancellationToken);

            if (result.NoReason)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(ModeratorFull), new { id });
            }

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Объект отклонен";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при отклонении объекта";
            }

            return RedirectToAction(nameof(ModeratorPanel), new { status = "Pending" });
        }
        #endregion

        #region ModeratorHistory История модерации
        [HttpGet]
        public async Task<IActionResult> ModeratorHistory(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await mediator.Send(new GetModeratorHistoryQuery
            {
                ModeratorId = user.Id
            }, cancellationToken);

            ViewBag.TotalCount = result.TotalCount;

            return View(result.History);
        }
        #endregion
    }
}