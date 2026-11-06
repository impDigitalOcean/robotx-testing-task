using Microsoft.AspNetCore.Mvc;

namespace ImportedDatabaseViewer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Client");
        }
    }
}
