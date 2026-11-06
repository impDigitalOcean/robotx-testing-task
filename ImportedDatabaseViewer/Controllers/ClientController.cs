using ImportedDatabaseViewer.Models;
using ImportedDatabaseViewer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImportedDatabaseViewer.Controllers
{
    public class ClientController : Controller
    {
        private readonly ClientRepository _repo;

        public ClientController(ClientRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var clients = await _repo.GetPagedAsync(page);
            return View(clients);
        }

        public async Task<IActionResult> Edit(int cardcode)
        {
            var client = await _repo.GetClientAsync(cardcode);
            return View(client);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Client client)
        {
            if(!ModelState.IsValid)
            {
                TempData["Error"] = "Неверные данные!";
                return RedirectToAction(nameof(Index));
            }
            await _repo.UpdateAsync(client);

            TempData["Message"] = $"Успешно обновлен клиент #{client.CardCode}!";
            return RedirectToAction(nameof(Index));
        }
    }
}
