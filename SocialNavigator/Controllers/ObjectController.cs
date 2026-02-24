using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    public class ObjectController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;

        public ObjectController(ILocalDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

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

        public IActionResult Add()
        {
            return View();
        }
    }
}
