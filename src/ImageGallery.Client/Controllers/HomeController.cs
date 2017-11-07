using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
