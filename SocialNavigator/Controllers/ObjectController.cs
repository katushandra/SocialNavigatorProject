using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SocialNavigator.Controllers
{
    public class ObjectController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ObjectController> logger;

        public ObjectController(ILocalDbContext context, IMapper mapper, UserManager<AppUser> userManager, ILogger<ObjectController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
        }

        #region Full карточка объекта
        public async Task<IActionResult> Full(Guid id, CancellationToken cancellationToken)
        {
            var objectDetails = await context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User)
                .Where(x => x.IdObject == id && x.Status == Domain.Entity.Enums.Status.Approved)
                .ProjectTo<FullSocialObjectDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return View(objectDetails);
        }
        #endregion

        #region Add добавление объекта
        public IActionResult Add()
        {
            return View();
        }
        #endregion

        #region Delete Удаление объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == id && x.CreatorId == user.Id);

            if (socialObject == null)
            {
                return NotFound();
            }

            if (socialObject.Status != Status.Pending)
            {
                TempData["ErrorMessage"] = "Удалить можно только объекты со статусом 'На модерации'";
                return RedirectToAction("MyObjects", "Profile");
            }

            context.SocialObject.Remove(socialObject);
            await context.SaveChangesAsync();

            logger.LogInformation("Пользователь {UserName} удалил объект {ObjectId}",
                user.UserName, id);

            TempData["SuccessMessage"] = "Объект успешно удален";
            return RedirectToAction("MyObjects", "Profile");
        }
        #endregion

        #region Edit Редактирование объекта
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var socialObject = await context.SocialObject
                .Include(x => x.ObjectType)
                .FirstOrDefaultAsync(x => x.IdObject == id && x.CreatorId == user.Id);

            if (socialObject == null)
            {
                return NotFound();
            }
            
            if (socialObject.Status != Status.Rejected)
            {
                TempData["ErrorMessage"] = "Редактировать можно только отклоненные объекты";
                return RedirectToAction("MyObjects", "Profile");
            }

            var editDto = mapper.Map<AddSocialObjectDto>(socialObject);

            ViewBag.ObjectId = id;
            return View("Edit", editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddSocialObjectDto model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ObjectId = id;
                return View("Edit", model);
            }

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == id && x.CreatorId == user.Id);

            if (socialObject == null)
            {
                return NotFound();
            }
            
            if (socialObject.Status != Status.Rejected)
            {
                TempData["ErrorMessage"] = "Редактировать можно только отклоненные объекты";
                return RedirectToAction("MyObjects", "Profile");
            }

            mapper.Map(model, socialObject);

            socialObject.Status = Status.Pending;
            socialObject.EditorId = user.Id;
            socialObject.EditedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Пользователь {UserName} отредактировал объект {ObjectId}",
                user.UserName, id);

            TempData["SuccessMessage"] = "Объект отправлен на повторную модерацию";
            return RedirectToAction("MyObjects", "Profile");
        }
        #endregion

    }
}
