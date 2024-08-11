using AcerProTask.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcerProTask.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }


        protected async Task<ActionResult<T>> GetByIdAsync<T>(int id) where T : class
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        protected async Task<ActionResult<IEnumerable<T>>> GetAllAsync<T>() where T : class
        {
            var entities = await _context.Set<T>().ToListAsync();
            return Ok(entities);
        }
    }
}
