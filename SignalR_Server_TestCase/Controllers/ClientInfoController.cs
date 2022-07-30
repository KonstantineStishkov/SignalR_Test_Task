using Entities;
using Microsoft.AspNetCore.Mvc;
using SignalR_Server_TestCase.Interfaces;

namespace SignalR_Server_TestCase.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            IEnumerable<ClientInfo> clients = new NpgSqlAdapter().GetInfo();
            ViewBag.Title = "All Clients";
            return View(clients);
        }
    }
}
