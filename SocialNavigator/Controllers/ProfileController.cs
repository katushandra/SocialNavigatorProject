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

        public ProfileController(IMapper mapper, ILogger<AccountController> logger, UserManager<AppUser> userManager)
        {
            this.mapper = mapper;
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

            var profileDto = mapper.Map<AppUserDto>(user);
            var roles = await userManager.GetRolesAsync(user);
            profileDto.Role = roles.FirstOrDefault() ?? "User";

            ViewBag.UserRoles = roles;
            return View("ProfileHome", profileDto);
        }
        #endregion

        #region UpdateProfile Обновление профиля

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
