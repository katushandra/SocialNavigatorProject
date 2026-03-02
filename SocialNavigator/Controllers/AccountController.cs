using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace SocialNavigator.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public AccountController(ILocalDbContext context, IMapper mapper, ILogger<AccountController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
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
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                logger.LogInformation("Пользователь {UserName} успешно создан", model.UserName);

                await userManager.AddToRoleAsync(user, "User");

                await signInManager.SignInAsync(user, isPersistent: false);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
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