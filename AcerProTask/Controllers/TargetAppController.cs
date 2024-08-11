using AcerProTask.Data;
using AcerProTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AcerProTask.Controllers
{
    public class TargetAppController : BaseController
    {
        protected readonly AppDbContext _context;

        public TargetAppController(AppDbContext context) : base(context)
        {
            _context = context;
        }


        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TargetApp targetApp)
        {
            ModelStateRemoveTargetApp();
            if (ModelState.IsValid)
            {
                targetApp.LastUpdated = DateTime.UtcNow;
                targetApp.UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                _context.Add(targetApp);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }


            return View(targetApp);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var targetApp = await _context.TargetApps.FindAsync(id);
            if (targetApp == null)
            {
                return NotFound();
            }
            return View(targetApp);
        }
        public void ModelStateRemoveTargetApp()
        {
            ModelState.Remove("HealthCheckLogs");
            ModelState.Remove("User");
            ModelState.Remove("UserId");
        }

        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TargetApp targetApp)
        {
            if (id != targetApp.Id)
            {
                return NotFound();
            }
            ModelStateRemoveTargetApp();
            if (ModelState.IsValid)
            {
                try
                {

                    targetApp.UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    targetApp.LastUpdated = DateTime.UtcNow;


                    _context.Update(targetApp);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TargetAppExists(targetApp.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            return View(targetApp);
        }


        [HttpPost, Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var targetApp = await _context.TargetApps.FindAsync(id);
            if (targetApp != null)
            {
                targetApp.IsDeleted = true;
                // _context.TargetApps.Remove(targetApp);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        private bool TargetAppExists(int id)
        {
            return _context.TargetApps.Any(e => e.Id == id);
        }
    }

}
