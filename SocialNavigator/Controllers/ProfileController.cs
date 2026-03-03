using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SocialNavigator.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IMapper mapper;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailService emailService;

        public ProfileController(IMapper mapper, ILogger<AccountController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
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

            var profileDto = mapper.Map<AppUserDto>(user);
            var roles = await userManager.GetRolesAsync(user);
            profileDto.Role = roles.FirstOrDefault() ?? "User";

            ViewBag.UserRoles = roles;
            return View("ProfileHome", profileDto);
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

            bool emailChange = false;

            if (user.FullName != model.FullName)
            {
                user.FullName = model.FullName;
            }

            if (user.Email != model.Email)
            {
                var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                {
                    ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует");
                    return View("ProfileHome", model);
                }
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpperInvariant();
                user.EmailConfirmed = false;
                emailChange = true;
            }

            user.UserEditedAt = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (emailChange)
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, email = model.Email, code = code },
                        protocol: HttpContext.Request.Scheme);

                    await emailService.SendEmailAsync(model.Email, "Подтверждение смены email", $"Для подтверждения нового email перейдите по ссылке: <a href='{callbackUrl}'>, чтобы подтвердить email</a>");

                    TempData["SuccessMessage"] = "На новый email отправлено письмо с подтверждением. Email будет изменен после подтверждения.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Профиль успешно обновлен";
                }

                logger.LogInformation("Пользователь {UserName} обновил профиль", user.UserName);
                await signInManager.RefreshSignInAsync(user);
                return RedirectToAction("ProfileHome");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.Email), error.Description);
            }
            return View("ProfileHome", model);
        }
        #endregion

        #region ChangePassword Изменение пароля

        #endregion

        #region MyObjects Мои объекты
        [HttpGet]
        public async Task<IActionResult> MyObjects()
        {
            return View();
        }
        #endregion

        #region MyReviews Мои отзывы
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            return View();
        }
        #endregion

        #region Moderator Панель модератора
        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Moderator()
        {
            return View();
        }
        #endregion

        #region Admin Панель администратора
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }
        #endregion
    }
}
