using Microsoft.AspNetCore.Mvc;

namespace ViperAppApi.Controllers
{
    public class PostsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
