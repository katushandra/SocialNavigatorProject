using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using Infrastructure.Geo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Org.BouncyCastle.Utilities;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SocialNavigator.Controllers
{
    public class ObjectController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ObjectController> logger;
        private readonly IGeoService geo;

        public ObjectController(ILocalDbContext context, IMapper mapper, UserManager<AppUser> userManager, ILogger<ObjectController> logger, IGeoService geo)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
            this.geo = geo;
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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var objectTypes = await context.ObjectType
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.ObjectTypes = objectTypes;

            return View(new AddSocialObjectDto());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddSocialObjectDto model) 
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var objectTypes = await context.ObjectType
                .OrderBy(x => x.Name)
                .ToListAsync();
            ViewBag.ObjectTypes = objectTypes;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var objectType = await context.ObjectType
                .FirstOrDefaultAsync(x => x.IdObjectType == model.ObjectTypeId);

            if (objectType == null)
            {
                ModelState.AddModelError(nameof(model.ObjectTypeId), "Выберите тип объекта");
                return View(model);
            }

            Point? location = null;
            if (!string.IsNullOrWhiteSpace(model.Address))
            {
                location = await geo.AddressСoordinate(model.Address);

                if (location == null)
                {
                    ModelState.AddModelError(nameof(model.Address), "Контроллер Не удалось определить координаты по указанному адресу. Пожалуйста, уточните адрес.");

                    return View(model);
                }
            }

            var socialObject = mapper.Map<SocialObject>(model);

            socialObject.Location = location;
            socialObject.CreatorId = user.Id;

            context.SocialObject.Add(socialObject);
            await context.SaveChangesAsync();

            logger.LogInformation("Пользователь {UserName} добавил новый объект {ObjectName} id - {ObjectId}",
                user.UserName, model.Name, socialObject.IdObject);

            TempData["SuccessMessage"] = "Объект спешно добавлен и отправлен на модерацию!";
            return RedirectToAction("MyObjects", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAddress([FromBody] AddCheckAddress model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.Address))
                {
                    return Json(new { success = false, message = "Адрес не указан" });
                }

                var location = await geo.AddressСoordinate(model.Address);

                if (location != null)
                {
                    var formattedAddress = await geo.СoordinateAddress(location.Y, location.X);

                    return Json(new
                    {
                        success = true,
                        message = "Контроллер CheckAddress Адрес найден",
                        address = formattedAddress ?? model.Address,
                        latitude = location.Y,
                        longitude = location.X
                    });
                }

                return Json(new { success = false, message = " Контроллер CheckAddress Адрес не найден. Попробуйте уточнить или выбрать из предложенных вариантов." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при проверке адреса: {Address}", model?.Address);
                return Json(new { success = false, message = "Контроллер CheckAddress Ошибка при проверке адреса" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckCordinate([FromBody] AddCheckCordinate model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Контроллер CheckCordinate Не указаны координаты" });
                }

                var address = await geo.СoordinateAddress(model.Latitude, model.Longitude);

                if (!string.IsNullOrEmpty(address))
                {
                    return Json(new
                    {
                        success = true,
                        address = address,
                        message = "Контроллер CheckCordinate Координаты определены"
                    });
                }

                return Json(new { success = false, message = " Контроллер CheckCordinate Не удалось определить адрес по координатам" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при чтении координат");
                return Json(new { success = false, message = " Контроллер CheckCordinate Ошибка при определении адреса" });
            }
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
            var objectTypes = await context.ObjectType
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.ObjectTypes = objectTypes;
            ViewBag.ObjectId = id;

            if (socialObject.Location != null)
            {
                ViewBag.Latitude = socialObject.Location.Y;
                ViewBag.Longitude = socialObject.Location.X;
            }

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

            if (socialObject.Address != model.Address && !string.IsNullOrWhiteSpace(model.Address))
            {
                var location = await geo.AddressСoordinate(model.Address);
                if (location != null)
                {
                    socialObject.Location = location;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Address), "Не удалось определить координаты по указанному адресу. Пожалуйста, уточните адрес.");

                    ViewBag.ObjectId = id;
                    var objectTypes = await context.ObjectType
                        .OrderBy(x => x.Name)
                        .ToListAsync();
                    ViewBag.ObjectTypes = objectTypes;
                    return View("Edit", model);
                }
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
