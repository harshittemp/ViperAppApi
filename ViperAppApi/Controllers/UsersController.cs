using Microsoft.AspNetCore.Mvc;

namespace ViperAppApi.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
