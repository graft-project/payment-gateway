using Graft.Infrastructure.Watcher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        readonly WatcherService _watcher;

        public AdminDashboardController(WatcherService watcher)
        {
            _watcher = watcher;
        }

        public IActionResult Index()
        {
            return View(_watcher);
        }
    }
}