using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
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

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            // Запрос на только одобренные объкты
            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.Status == Domain.Entity.Enums.Status.Approved)
                .AsQueryable();

            // Получение данных (новые, макс 10)
            var socialObjects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<SocialObjectDto>(mapper.ConfigurationProvider)
                .Take(10)
                .ToListAsync(cancellationToken);

            return View(socialObjects);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
