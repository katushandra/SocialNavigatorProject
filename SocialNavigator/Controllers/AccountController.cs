using Application.Handlers.Account.Commands;
using Domain.DTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialNavigator.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator mediator;
        private readonly ILogger<AccountController> logger;

        public AccountController(IMediator mediator, ILogger<AccountController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
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

            var result = await mediator.Send(new LoginCommand
            {
                Model = model,
                ReturnUrl = returnUrl
            });

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                ModelState.AddModelError(nameof(model.Password), result.Error);
            }
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

            logger.LogInformation("Начало регистрации для пользователя {UserName}", model.UserName);

            var result = await mediator.Send(new RegisterCommand
            {
                Model = model,
                UrlAction = (userId, email, code) => Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = userId, email = email, code = code },
                    protocol: HttpContext.Request.Scheme) ?? "#"
            });

            if (result.Succeeded && result.ShowConfirmation)
            {
                logger.LogInformation("Регистрация успешна для {UserName}, отправлено письмо на {Email}", model.UserName, result.Email);
                ViewBag.Email = result.Email;
                return View("RegisterConf");
            }

            if (result.Errors.Any())
            {
                logger.LogWarning("Ошибки регистрации для {UserName}: {Errors}", model.UserName, string.Join("; ", result.Errors));

                foreach (var error in result.Errors)
                {

                    if (error.Contains("Пароль") || error.Contains("должен содержать"))
                    {
                        ModelState.AddModelError(nameof(model.Password), error);
                    }
                    else if (error.Contains("Email") || error.Contains("email"))
                    {
                        ModelState.AddModelError(nameof(model.Email), error);
                    }
                    else if (error.Contains("UserName") || error.Contains("Имя пользователя") || error.Contains("Логин"))
                    {
                        ModelState.AddModelError(nameof(model.UserName), error);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                logger.LogWarning("ConfirmEmail: userId или code пустые");
                return View("Error");
            }

            var result = await mediator.Send(new ConfirmEmailCommand
            {
                UserId = userId,
                Code = code
            });

            if (result.UserNotFound)
            {
                logger.LogWarning("ConfirmEmail: пользователь не найден {UserId}", userId);
                return View("Error");
            }

            if (!result.Succeeded)
            {
                logger.LogWarning("ConfirmEmail: не удалось подтвердить email для {UserId}", userId);
                return View("Error");
            }

            logger.LogInformation("ConfirmEmail: email успешно подтвержден для {UserId}", userId);
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Logout Выход
        public async Task<IActionResult> Logout()
        {
            await mediator.Send(new LogoutCommand());
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        #endregion

        #region ForgotPassword Забыли пароль
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await mediator.Send(new ForgotPasswordCommand
            {
                Model = model,
                UrlAction = (userId, email, code) => Url.Action(
                     "ResetPassword",
                     "Account",
                     new { userId = userId, email = email, code = code },
                     protocol: HttpContext.Request.Scheme) ?? "#"
            });

            return View("ForgotPasswordConf");
        }
        #endregion

        #region ResetPassword Сброс пароля
        [HttpGet]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return View("Error");
            }

            var model = new ResetPasswordDto { Code = code, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var result = await mediator.Send(new ResetPasswordCommand { Model = model });

            foreach (var error in result.Errors)
            {
                if (error.Contains("Пароль") || error.Contains("должен содержать"))
                {
                    ModelState.AddModelError(nameof(model.Password), error);
                }
                else if (error.Contains("совпадают"))
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), error);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            if (!result.Succeeded)
            {
                return View(model);
            }

            return RedirectToAction("ResetPasswordConf");
        }

        [HttpGet]
        public IActionResult ResetPasswordConf()
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