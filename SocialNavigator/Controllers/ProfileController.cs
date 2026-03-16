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
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IMapper mapper;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailService emailService;
        private readonly ILocalDbContext context;

        public ProfileController(IMapper mapper, ILogger<AccountController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, ILocalDbContext context)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.context = context;
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

            if (roles.Contains("Moderator") || roles.Contains("Admin"))
            {
                var pendingCount = await context.SocialObject
                    .CountAsync(x => x.Status == Status.Pending);
                ViewBag.PendingCount = pendingCount;
            }

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

            bool hasChange = false;
            bool emailChange = false;

            if (user.FullName != model.FullName)
            {
                user.FullName = model.FullName;
                hasChange = true;
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
                hasChange = true;
            }

            if (!hasChange)
            {
                TempData["InfoMessage"] = "Нет изменений для сохранения";
                return RedirectToAction("ProfileHome");
            }
            else
            {
                user.UserEditedAt = DateTime.UtcNow;
            }

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

                    await emailService.SendEmailAsync(model.Email, "Подтверждение смены email", $@"
                    Здравствуйте, {user.FullName ?? user.UserName}!
                    <br><br>
                    Для подтверждения нового email перейдите по ссылке <a href='{callbackUrl}'> подтвердить email</a>
                    <br><br>
                    С уважением,<br>
                    Команда Социальный навигатор!");

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

            var checkPassword = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!checkPassword)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Неверный текущий пароль");
                return View(model);
            }

            if (model.CurrentPassword == model.NewPassword)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Новый пароль должен отличаться от текущего");
                return View(model);
            }

            var newPasswordErrors = ValidatorPassword(model.NewPassword);
            foreach (var error in newPasswordErrors)
            {
                ModelState.AddModelError(nameof(model.NewPassword), error);
            }

            var confirmPasswordErrors = ValidatorPassword(model.ConfirmPassword);
            foreach (var error in confirmPasswordErrors)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), error);
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "Пароль успешно изменен";
                logger.LogInformation("Пользователь {UserName} успешно сменил пароль", user.UserName);
                await signInManager.RefreshSignInAsync(user);
                return RedirectToAction("ProfileHome");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), error.Description);
            }

            return View(model);
        }
        // валидация пароля
        private List<string> ValidatorPassword(string password)
        {
            var errors = new List<string>();

            if (password.Length < 6)
            {
                errors.Add("Пароль должен быть не менее 6 символов");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Пароль должен содержать хотя бы одну цифру");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Пароль должен содержать хотя бы одну строчную букву");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Пароль должен содержать хотя бы одну заглавную букву");
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                errors.Add("Пароль должен содержать хотя бы один специальный символ");
            }
            return errors;
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

            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.CreatorId == user.Id);

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

            var userObjects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<MyObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            ViewBag.CurrentStatus = status;
            ViewBag.StatusCounts = statusCounts;
            ViewBag.TotalCount = userObjects.Count;
            return View(userObjects);
        }
        #endregion

        #region MyReviews Мои отзывы
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            return View();
        }
        #endregion
        
    }
}
