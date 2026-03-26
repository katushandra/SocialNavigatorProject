using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public HomeController(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string objectType = "", string accessibility = "", CancellationToken cancellationToken = default)
        {
            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.Status == Status.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.Address.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(objectType))
            {
                query = query.Where(x => x.ObjectType.Name == objectType);
            }

            if (!string.IsNullOrWhiteSpace(accessibility))
            {
                switch (accessibility)
                {
                    case "wheelchairs":
                        query = query.Where(x => x.Wheelchairs);
                        break;
                    case "blind":
                        query = query.Where(x => x.BlindAccess);
                        break;
                    case "deaf":
                        query = query.Where(x => x.DeafAccess);
                        break;
                    case "speech":
                        query = query.Where(x => x.SpeechAccess);
                        break;
                    case "mobility":
                        query = query.Where(x => x.MobilityAccess);
                        break;
                    case "intellectual":
                        query = query.Where(x => x.IntellectualAccess);
                        break;
                    case "autism":
                        query = query.Where(x => x.AutismAccess);
                        break;
                }
            }

            var socialObjects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<SocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var mapObjects = socialObjects
                .Where(x => x.Location != null)
                .Select(x => new
                {
                    x.IdObject,
                    x.Name,
                    x.Address,
                    x.ScoreObject,
                    Latitude = x.Location.Y,
                    Longitude = x.Location.X,
                    ObjectTypeName = x.ObjectType?.NameObjectType,
                    Accessibility = new
                    {
                        x.Wheelchairs,
                        x.BlindAccess,
                        x.DeafAccess,
                        x.SpeechAccess,
                        x.MobilityAccess,
                        x.IntellectualAccess,
                        x.AutismAccess
                    }
                })
                .ToList();

            var objectTypes = await context.ObjectType
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.ObjectTypeFilter = objectType;
            ViewBag.AccessibilityFilter = accessibility;
            ViewBag.ObjectTypes = objectTypes;
            ViewBag.MapObjects = mapObjects;

            return View(socialObjects);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}