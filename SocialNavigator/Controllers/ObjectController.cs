using Application.Common.Interfaces;
using Application.Handlers.Object.Commands;
using Application.Handlers.Object.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Domain.Entity.Enums;
using MediatR;
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
        private readonly IMediator mediator;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ObjectController> logger;

        public ObjectController(IMediator mediator, ILocalDbContext context, IMapper mapper, UserManager<AppUser> userManager, ILogger<ObjectController> logger)
        {
            this.mediator = mediator;
            this.userManager = userManager;
            this.logger = logger;
        }

        #region Full карточка объекта
        public async Task<IActionResult> Full(Guid id, CancellationToken cancellationToken)
        {
            var objectDetails = await mediator.Send(new GetFullQuery { Id = id }, cancellationToken);

            if (objectDetails == null)
            {
                return NotFound();
            }

            return View(objectDetails);
        }
        #endregion

        #region Add добавление объекта
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Add(CancellationToken cancellationToken)
        {
            var objectTypes = await mediator.Send(new GetObjectTypesQuery(), cancellationToken);

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

            var objectTypes = await mediator.Send(new GetObjectTypesQuery(), cancellationToken);
            ViewBag.ObjectTypes = objectTypes;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            double? latitude = null;
            double? longitude = null;
                     
            if (Request.Form.ContainsKey("Latitude") && Request.Form.ContainsKey("Longitude"))
            {
                if (double.TryParse(Request.Form["Latitude"], out double lat) &&
                    double.TryParse(Request.Form["Longitude"], out double lng))
                {
                    latitude = lat;
                    longitude = lng;
                }
            }

            var result = await mediator.Send(new AddCommand
            {
                Model = model,
                UserId = user.Id,
                Latitude = latitude,
                Longitude = longitude
            }, cancellationToken);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.ObjectTypeId), result.Error ?? "Ошибка при добавлении объекта");
                return View(model);
            }

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

            var result = await mediator.Send(new DeleteObjectCommand
            {
                Id = id,
                UserId = user.Id
            });

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.InvalidStatus)
            {
                TempData["ErrorMessage"] = result.Error;
            }
            else if (result.Success)
            {
                TempData["SuccessMessage"] = "Объект успешно удален";
            }

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

            var editDto = await mediator.Send(new GetForEditQuery
            {
                Id = id,
                UserId = user.Id
            });

            if (editDto == null)
            {
                return NotFound();
            }

            var objectTypes = await mediator.Send(new GetObjectTypesQuery());
            ViewBag.ObjectTypes = objectTypes;
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
                var objectTypes = await mediator.Send(new GetObjectTypesQuery());
                ViewBag.ObjectTypes = objectTypes;
                return View("Edit", model);
            }

            double? latitude = null;
            double? longitude = null;

            if (Request.Form.ContainsKey("Latitude") && Request.Form.ContainsKey("Longitude"))
            {
                if (double.TryParse(Request.Form["Latitude"], out double lat) &&
                    double.TryParse(Request.Form["Longitude"], out double lng))
                {
                    latitude = lat;
                    longitude = lng;
                }
            }

            var result = await mediator.Send(new EditObjectCommand
            {
                Id = id,
                Model = model,
                UserId = user.Id,
                Latitude = latitude,
                Longitude = longitude
            });

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.InvalidStatus)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("MyObjects", "Profile");
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Объект отправлен на повторную модерацию";
            }

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
                var fullObjectDto = await mediator.Send(new GetFullQuery { Id = model.ObjectId }, cancellationToken);
                if (fullObjectDto == null)
                {
                    return NotFound();
                }

                fullObjectDto.AddReview = model;
                var objectTypes = await mediator.Send(new GetObjectTypesQuery(), cancellationToken);
                ViewBag.ObjectTypes = objectTypes;

                return View("Full", fullObjectDto);
            }

            var result = await mediator.Send(new AddReviewCommand
            {
                Model = model,
                UserId = user.Id
            }, cancellationToken);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error ?? "Ошибка при добавлении отзыва";
                return RedirectToAction(nameof(Full), new { id = model.ObjectId });
            }

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

            var result = await mediator.Send(new DeleteReviewCommand
            {
                Id = id,
                ObjectId = objectId,
                UserId = user.Id
            }, cancellationToken);

            if (result.NotFound)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Отзыв удален";
            return RedirectToAction(nameof(Full), new { id = objectId });
        }
        #endregion
    }
}
