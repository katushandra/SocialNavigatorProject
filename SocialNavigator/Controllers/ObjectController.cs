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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public async Task<IActionResult> Add(AddSocialObjectDto model, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var objectTypes = await context.ObjectType
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
            ViewBag.ObjectTypes = objectTypes;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var objectType = await context.ObjectType
                .FirstOrDefaultAsync(x => x.IdObjectType == model.ObjectTypeId, cancellationToken);

            if (objectType == null)
            {
                ModelState.AddModelError(nameof(model.ObjectTypeId), "Выберите тип объекта");
                return View(model);
            }

            var socialObject = mapper.Map<SocialObject>(model);

            if (Request.Form.ContainsKey("Latitude") && Request.Form.ContainsKey("Longitude"))
            {
                if (double.TryParse(Request.Form["Latitude"], out double latitude) &&
                    double.TryParse(Request.Form["Longitude"], out double longitude))
                {
                    var point = new Point(longitude, latitude)  // x - долгота, y - широта
                    {
                        SRID = 4326
                    };
                    socialObject.Location = point;
                }
            }

            socialObject.CreatorId = user.Id;

            context.SocialObject.Add(socialObject);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Пользователь {UserName} добавил новый объект {ObjectName} id - {ObjectId}",
                user.UserName, model.Name, socialObject.IdObject);

            TempData["SuccessMessage"] = "Объект успешно добавлен и отправлен на модерацию!";
            return RedirectToAction("MyObjects", "Profile");
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
                .Select(x => new { x.IdObjectType, x.Name })
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
                var objectTypes = await context.ObjectType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                ViewBag.ObjectTypes = objectTypes;
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

            if (Request.Form.ContainsKey("Latitude") && Request.Form.ContainsKey("Longitude"))
            {
                if (double.TryParse(Request.Form["Latitude"], out double latitude) &&
                    double.TryParse(Request.Form["Longitude"], out double longitude))
                {
                    var point = new Point(longitude, latitude)  // x - долгота, y - широта
                    {
                        SRID = 4326
                    };
                    socialObject.Location = point;
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

        #region AddReview Добавление отзыва
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewDto model, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var fullObjectDto = await context.SocialObject
                   .Include(x => x.ObjectType)
                   .Include(x => x.Reviews)
                       .ThenInclude(r => r.User)
                   .Where(x => x.IdObject == model.ObjectId && x.Status == Status.Approved)
                   .ProjectTo<FullSocialObjectDto>(mapper.ConfigurationProvider)
                   .FirstOrDefaultAsync(cancellationToken);

                if (fullObjectDto == null)
                {
                    return NotFound();
                }

                // Заполняем данные из формы в свойство NewReview
                fullObjectDto.AddReview = model;

                // Добавляем объекты в ViewBag для выпадающих списков, если они нужны
                var objectTypes = await context.ObjectType
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                ViewBag.ObjectTypes = objectTypes;

                return View("Full", fullObjectDto);
            }

            var review = mapper.Map<Review>(model);
            review.UserId = user.Id;

            context.Review.Add(review);
            await context.SaveChangesAsync(cancellationToken);

            await UpdateObjectScore(model.ObjectId, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Пользователь {UserName} добавил отзыв на объект {ObjectId}",
                user.UserName, model.ObjectId);

            TempData["SuccessMessage"] = "Отзыв успешно добавлен";
            return RedirectToAction(nameof(Full), new { id = model.ObjectId });
        }
        #endregion

        #region DeleteReview Удаление отзыва
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(Guid id, Guid objectId, CancellationToken cancellationToken)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = await context.Review
                .FirstOrDefaultAsync(x => x.IdReview == id && x.UserId == user.Id, cancellationToken);

            if (review == null)
            {
                return NotFound();
            }

            context.Review.Remove(review);

            await UpdateObjectScore(objectId, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Пользователь {UserName} удалил отзыв {ReviewId}",
                user.UserName, id);

            TempData["SuccessMessage"] = "Отзыв удален";
            return RedirectToAction(nameof(Full), new { id = objectId });
        }

        // Обновление средней оценки объекта
        private async Task UpdateObjectScore(Guid objectId, CancellationToken cancellationToken)
        {
            var averageScore = await context.Review
                .Where(x => x.ObjectId == objectId)
                .AverageAsync(x => (decimal?)x.Score, cancellationToken) ?? 0;

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == objectId, cancellationToken);

            if (socialObject != null)
            {
                socialObject.ScoreObject = Math.Round(averageScore, 1);
            }
        }
        #endregion
    }
}
