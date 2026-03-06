using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace SocialNavigator.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMapper mapper;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailService emailService;

        public AccountController(IMapper mapper, ILogger<AccountController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }

        #region Login Вход
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                if (!await userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Вы не подтвердили свой email");
                    return View(model);
                }
            }

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Неверное имя пользователя или пароль.");
                return View(model);
            }

            if (!user.Active)
            {
                ModelState.AddModelError(nameof(model.UserName), "Ваш аккаунт деактивирован. Обратитесь к администратору");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: true); //false - не запоминать, lockoutOnFailure: true - блокировать при ошибках

            if (result.Succeeded)
            {
                logger.LogInformation("Пользователь {UserName} успешно вошел в систему", model.UserName);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning("Аккаунт {UserName} заблокирован", model.UserName);
                ModelState.AddModelError(nameof(model.UserName), "Аккаунт временно заблокирован. Попробуйте позже.");
                return View(model);
            }

            ModelState.AddModelError(nameof(model.Password), "Неверное имя пользователя или пароль.");
            return View(model);
        }
        #endregion

        #region Register Регистрация
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var passwordErrors = ValidatorPassword(model.Password);
            foreach (var error in passwordErrors)
            {
                ModelState.AddModelError(nameof(model.Password), error);
            }

            var confirmPasswordErrors = ValidatorPassword(model.PasswordConfirm);
            foreach (var error in confirmPasswordErrors)
            {
                ModelState.AddModelError(nameof(model.PasswordConfirm), error);
            }

            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует");
                return View(model);
            }

            var existingUserByUserName = await userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Пользователь с таким именем уже существует");
                return View(model);
            }


            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                UserCreatedAt = DateTime.UtcNow,
                Active = true,
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                logger.LogInformation("Пользователь {UserName} создан", model.UserName);

                await userManager.AddToRoleAsync(user, "User");

                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, code = code },
                    protocol: HttpContext.Request.Scheme);

                await emailService.SendEmailAsync(model.Email, "Подтверждение регистрации", $@"
                Здравствуйте, {model.FullName}!
                <br><br>
                Вы успешно зарегистрировались на сайте Социальный навигатор!<br>
                Для завершения регистрации перейдите по ссылке <a href='{callbackUrl}'> подтвердить email</a>
                <br><br>
                С уважением,<br>
                Команда Социальный навигатор!");

                ViewBag.Email = model.Email;
                return View("~/Views/Account/RegisterConf.cshtml");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(model.PasswordConfirm), error.Description);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return RedirectToAction("Login", "Account");
            else
                return View("Error");
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

        #region Logout Выход
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        #endregion

        #region ForgotPassword Забыли пароль
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        #endregion

        #region AccessDenied Доступ запрещен
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion
    }
}