using Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace SocialNavigator.Controllers
{
    public class ObjectController : Controller
    {
        public IActionResult Add()
        {
            return View();
        }
    }
}
