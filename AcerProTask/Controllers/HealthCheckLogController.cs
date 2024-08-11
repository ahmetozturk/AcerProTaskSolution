using AcerProTask.Data;
using AcerProTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcerProTask.Controllers
{
    public class HealthCheckLogController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10; 

        public HealthCheckLogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? filterDate, string filterAppName, int page = 1)
        {
            var model = new HealthCheckLogViewModel();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var uuid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var query = _context.HealthCheckLogs
                    .Where(x => x.TargetApp.UserId == uuid)
                .Include(log => log.TargetApp)
                .AsQueryable();

                if (filterDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp.Date == filterDate.Value.Date);
                }

                if (!string.IsNullOrEmpty(filterAppName))
                {
                    query = query.Where(log => log.TargetApp.Name.Contains(filterAppName));
                }

                var totalLogs = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalLogs / (double)PageSize);

                var logs = await query
                    .OrderByDescending(log => log.Timestamp)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                model = new HealthCheckLogViewModel
                {
                    HealthCheckLogs = logs,
                    FilterDate = filterDate ?? DateTime.Today,
                    FilterAppName = filterAppName,
                    CurrentPage = page,
                    TotalPages = totalPages
                };
            }
            return View(model);
        }
    }
}
