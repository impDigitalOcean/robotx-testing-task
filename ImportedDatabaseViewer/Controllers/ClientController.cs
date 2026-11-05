using Microsoft.AspNetCore.Mvc;

namespace ImportedDatabaseViewer.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
