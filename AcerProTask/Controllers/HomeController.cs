using AcerProTask.Data;
using AcerProTask.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AcerProTask.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        protected readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context) : base(context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/RegisterAndLogin");
            }

            List<TargetApp> activeApps = new List<TargetApp>();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {


                var uuid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


                activeApps = _context.TargetApps
                .Where(app => app.MonitoringInterval > 0 && app.UserId == uuid && !app.IsDeleted)
                .ToList();

            }


            return View(activeApps);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
