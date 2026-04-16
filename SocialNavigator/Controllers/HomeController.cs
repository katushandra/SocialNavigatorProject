using Application.Common.Interfaces;
using Application.Handlers.Home.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator mediator;

        public HomeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index(string searchTerm = "", string objectType = "", string accessibility = "", CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetHomePageQuery
            {
                SearchTerm = searchTerm,
                ObjectType = objectType,
                Accessibility = accessibility
            }, cancellationToken);

            ViewBag.SearchTerm = result.SearchTerm;
            ViewBag.ObjectTypeFilter = result.ObjectTypeFilter;
            ViewBag.AccessibilityFilter = result.AccessibilityFilter;
            ViewBag.ObjectTypes = result.ObjectTypes;
            ViewBag.MapObjects = result.MapObjects;

            return View(result.SocialObjects);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}