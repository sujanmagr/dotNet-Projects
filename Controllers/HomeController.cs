using FoodY.Data;
using FoodY.Models;
using FoodY.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodY.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }



        public async Task<IActionResult> Index(string search)
        {
            var items = from i in _context.Products.Include(i => i.Category)
                        select i;
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(s => s.Name.Contains(search) || s.Category.Name.Contains(search));
            }

            var itemViewModels = await items.Select(item => new ProductViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.Name,
                ImageFileNames = _context.ProductImages
                    .Where(img => img.ProductId == item.Id)
                    .Select(img => img.FileName)
                    .ToList()
            }).ToListAsync();

            return View(itemViewModels);
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
        // Action for Checkout page
        public IActionResult Checkout()
        {
            return View();
        }
    }
}