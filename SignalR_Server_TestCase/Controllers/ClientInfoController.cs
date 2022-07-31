using Entities;
using Microsoft.AspNetCore.Mvc;
using SignalR_Server_TestCase.Models;

namespace SignalR_Server_TestCase.Controllers
{
    public class ClientInfoController : Controller
    {
        public IActionResult Index()
        {
            IEnumerable<ClientInfoModel> clients = new NpgSqlAdapter().GetInfo();
            ViewBag.Title = "All Clients";
            return View(clients);
        }
    }
}
