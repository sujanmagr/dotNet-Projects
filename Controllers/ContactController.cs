using FoodY.Data;
using FoodY.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodY.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Contact
        public async Task<IActionResult> Index()
        {
            var c = await _context.ContactUs
                .Select(c => new ContactViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Message = c.Message,
                    Email = c.Email,   
                    Subject = c.Subject,
                    Mobile = c.Mobile,
                })
                .ToListAsync();

            return View(c);
        }
    }
}
