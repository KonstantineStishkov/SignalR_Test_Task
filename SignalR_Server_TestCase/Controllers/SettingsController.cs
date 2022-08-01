using Microsoft.AspNetCore.Mvc;
using SignalR_Server_TestCase.Models;

namespace SignalR_Server_TestCase.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SetPeriod(string value)
        {
            OverallInfo.Period = TimeSpan.FromSeconds(int.Parse(value));
            return Index();
        }
    }
}
