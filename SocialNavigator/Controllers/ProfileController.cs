using Application.Common.Interfaces;
using Application.Handlers.Profile.Commands;
using Application.Handlers.Profile.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IMediator mediator;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<AppUser> userManager;

        public ProfileController(IMediator mediator, UserManager<AppUser> userManager, ILogger<AccountController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
            this.userManager = userManager;
        }

        #region Profile Профиль
        [HttpGet]
        public async Task<IActionResult> ProfileHome()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await mediator.Send(new GetProfileQuery { UserId = user.Id });

            if (result?.Profile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.PendingCount = result.PendingCount;
            ViewBag.UserRoles = result.Roles;

            return View("ProfileHome", result.Profile);
        }
        #endregion

        #region UpdateProfile Обновление профиля
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AppUserDto model)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            model.Role = roles.FirstOrDefault() ?? "User";
            ViewBag.EmailConfirmed = await userManager.IsEmailConfirmedAsync(user);

            if (!ModelState.IsValid)
            {
                return View("ProfileHome", model);
            }

            var result = await mediator.Send(new UpdateProfileCommand
            {
                Model = model,
                UserId = user.Id,
                UrlAction = (userId, email, code) => Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = userId, email = email, code = code },
                    protocol: HttpContext.Request.Scheme)
            });

            if (result.EmailAlreadyExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует");
                roles = await userManager.GetRolesAsync(user);
                model.Role = roles.FirstOrDefault() ?? "User";
                return View("ProfileHome", model);
            }

            if (result.NoChanges)
            {
                TempData["InfoMessage"] = "Изменений не обнаружено. Данные профиля остались без изменений.";
                return View("ProfileHome", model);
            }

            if (result.Succeeded)
            {
                if (result.EmailChanged)
                {
                    TempData["SuccessMessage"] = "На новый email отправлено письмо с подтверждением. Email будет изменен после подтверждения.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Профиль успешно обновлен";
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(nameof(model.Email), error);
                }
                roles = await userManager.GetRolesAsync(user);
                model.Role = roles.FirstOrDefault() ?? "User";
                return View("ProfileHome", model);
            }

            return View("ProfileHome", model);
        }
        #endregion

        #region ChangePassword Изменение пароля
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var user = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await mediator.Send(new ChangePasswordCommand
            {
                Model = model,
                UserId = user.Id
            });

            if (result.CurrentPasswordInvalid)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Неверный текущий пароль");
                return View(model);
            }

            if (result.SamePassword)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Новый пароль должен отличаться от текущего");
                return View(model);
            }

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), error);
                }
                return View(model);
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Пароль успешно изменен";
                return RedirectToAction("ProfileHome");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), error);
            }

            return View(model);
        }        
        #endregion

        #region MyObjects Мои объекты
        [HttpGet]
        public async Task<IActionResult> MyObjects(CancellationToken cancellationToken, string status)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await mediator.Send(new GetMyObjectsQuery
            {
                UserId = user.Id,
                Status = status
            }, cancellationToken);

            ViewBag.CurrentStatus = result.CurrentStatus;
            ViewBag.StatusCounts = result.StatusCounts;
            ViewBag.TotalCount = result.TotalCount;

            return View(result.Objects);
        }
        #endregion

        #region MyReviews Мои отзывы
        [HttpGet]
        public async Task<IActionResult> MyReviews(CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await mediator.Send(new GetMyReviewsQuery
            {
                UserId = user.Id
            }, cancellationToken);

            return View(result.Reviews);
        }
        #endregion
    }
}