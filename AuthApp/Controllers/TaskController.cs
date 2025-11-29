using AuthApp.Data;
using AuthApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace AuthApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            int AccountId = Convert.ToInt32(HttpContext.Session.GetInt32("AccountId"));
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == AccountId)
                .ToListAsync();
            return View(tasks);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveTask(int Id)
        {
            int AccountId = Convert.ToInt32(HttpContext.Session.GetInt32("AccountId"));
            var task = await _context.Tasks
                .Where(t => t.AccountId == AccountId)
                .FirstOrDefaultAsync(t => t.Id == Id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == AccountId)
                .ToListAsync();
            return View("Index", tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string description)
        {
            TaskJob t = new TaskJob();
            t.Name = name;
            t.Description = description;
            t.CategoryId = 1;
            t.AccountId = Convert.ToInt32(HttpContext.Session.GetInt32("AccountId"));
            int AccountId = Convert.ToInt32(HttpContext.Session.GetInt32("AccountId"));
            _context.Tasks.Add(t);
            await _context.SaveChangesAsync();
            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.AccountId == AccountId)
                .ToListAsync();
            return View("Index", tasks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(int id, string name, string description)
        {
            try
            {
                int AccountId = Convert.ToInt32(HttpContext.Session.GetInt32("AccountId"));
                var tasks = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.AccountId == AccountId);
                if (tasks != null)
                {
                    tasks.Name = name;
                    tasks.Description = description;
                    _context.SaveChanges();

                }
                return Json(new { success = true});
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }


        }
    }
}
