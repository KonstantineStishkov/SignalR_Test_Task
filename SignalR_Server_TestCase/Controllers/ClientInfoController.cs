using Microsoft.AspNetCore.Mvc;
using SignalR_Server_TestCase.Interfaces;

namespace SignalR_Server_TestCase.Controllers
{
    public class ClientInfoController : Controller
    {
        private readonly IAllClients allClients;
        private readonly IAllStorages allStorages;

        public ClientInfoController(IAllClients allClients, IAllStorages allStorages)
        {
            this.allClients = allClients;
            this.allStorages = allStorages;
        }

        public IActionResult Index()
        {
            IEnumerable<Models.ClientInfoModel>? clients = allClients.AllClients;
            ViewBag.Head = "All Clients";
            return View(clients);
        }
    }
}
