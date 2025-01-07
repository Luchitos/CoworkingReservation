using Microsoft.AspNetCore.Mvc;

namespace CoworkingReservation.API.Controllers
{
    public class CoworkingSpaceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
