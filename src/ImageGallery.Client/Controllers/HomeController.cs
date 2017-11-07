using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.Client.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult About()
        {
            return View();
        }
    }
}
