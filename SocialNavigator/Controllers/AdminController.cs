using Application.Handlers.Admin.Commands;
using Application.Handlers.Admin.Queries;
using AutoMapper;
using Domain.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMediator mediator;
        private readonly ILogger<AdminController> logger;

        public AdminController(IMediator mediator, ILogger<AdminController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        #region AdminPanel Панель администратора
        [HttpGet]
        public async Task<IActionResult> AdminPanel(CancellationToken cancellationToken)
        {
            var statistics = await mediator.Send(new GetAdminPanelQuery(), cancellationToken);
            return View(statistics);
        }
        #endregion

        #region AdminAllObjects Управление объектами
        [HttpGet]
        public async Task<IActionResult> AdminAllObjects(string searchTerm = "", string status = "", string type = "", CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetAdminAllObjectsQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                Type = type
            }, cancellationToken);

            ViewBag.SearchTerm = result.SearchTerm;
            ViewBag.StatusFilter = result.Status;
            ViewBag.TypeFilter = result.Type;
            ViewBag.ObjectTypes = result.ObjectTypes;

            return View(result.Objects);
        }

        // Удаление объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteObject(Guid id)
        {
            var result = await mediator.Send(new DeleteObjectCommand { Id = id });

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Объект успешно удален";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при удалении";
            }

            return RedirectToAction(nameof(AdminAllObjects));
        }
        #endregion

        #region AdminModerationHistory История модерации
        [HttpGet]
        public async Task<IActionResult> AdminModerationHistory(string searchTerm = "", CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetAdminModerationHistoryQuery
            {
                SearchTerm = searchTerm
            }, cancellationToken);

            ViewBag.SearchTerm = result.SearchTerm;
            return View(result.History);
        }
        #endregion

        #region AdminUsers Управление пользователями
        [HttpGet]
        public async Task<IActionResult> AdminUsers(string searchTerm = "", string role = "", bool? active = null, CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetAdminUsersQuery
            {
                SearchTerm = searchTerm,
                Role = role,
                Active = active
            }, cancellationToken);

            ViewBag.SearchTerm = result.SearchTerm;
            ViewBag.RoleFilter = result.RoleFilter;
            ViewBag.ActiveFilter = result.ActiveFilter;
            ViewBag.Roles = result.Roles;

            return View(result.Users);
        }

        // Изменение статуса пользователя
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserStatus(Guid userId)
        {
            var result = await mediator.Send(new ChangeUserStatusCommand { UserId = userId });

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Success)
            {
                if (result.NewActiveStatus)
                {
                    TempData["SuccessMessage"] = "Пользователь активирован";
                }
                else
                {
                    TempData["SuccessMessage"] = "Пользователь деактивирован";
                }
            }
            else
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при изменении статуса";
            }

            return RedirectToAction(nameof(AdminUsers));
        }

        // Изменение роли пользователя
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(Guid userId, string newRole)
        {
            var result = await mediator.Send(new ChangeUserRoleCommand
            {
                UserId = userId,
                NewRole = newRole
            });

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Роль пользователя изменена";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при изменении роли";
            }

            return RedirectToAction(nameof(AdminUsers));
        }
        #endregion
    }
}