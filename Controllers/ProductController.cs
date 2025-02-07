using FoodY.Data;
using FoodY.Models;
using FoodY.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodY.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        //get product 
        //public async Task<IActionResult> Index()
        //{
        //    var items = await _context.Products
        //        .Select(i => new ProductViewModel
        //        {
        //            Id = i.Id,
        //            Name = i.Name,
        //            Price = i.Price,
        //            Description = i.Description,
        //            CategoryId = i.CategoryId,
        //            CategoryName = i.Category.Name,
        //            ImageFileNames = i.Images.Select(img => img.FileName).ToList(),

        //        })
        //        .ToListAsync();
        //    var categories = await _context.Categories.Select(c => new CategoryViewModel
        //    {
        //        Id = c.Id,
        //        Name = c.Name
        //    }).ToListAsync();

        //    ViewBag.Categories = categories;
        //    return View(items);
        //}
        public async Task<IActionResult> Index(SearchViewModel searchModel)
        {
            var query = _context.Products.Include(i => i.Category).Include(i => i.Images).AsQueryable();
            // Filter based on IsActive status



            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(i => i.Name.Contains(searchModel.Name));
            }

            if (searchModel.CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == searchModel.CategoryId.Value);
            }

            if (searchModel.MinPrice.HasValue)
            {
                query = query.Where(i => i.Price >= searchModel.MinPrice.Value);
            }

            if (searchModel.MaxPrice.HasValue)
            {
                query = query.Where(i => i.Price <= searchModel.MaxPrice.Value);
            }

            var items = await query.Select(i => new ProductViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Price = i.Price,
                Description = i.Description,
                CategoryId = i.CategoryId,
                CategoryName = i.Category.Name,
                ImageFileNames = i.Images.Select(img => img.FileName).ToList(),

            }).ToListAsync();

            var categories = await _context.Categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToListAsync();

            ViewBag.Categories = categories;
            return View(items);
        }
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(new ProductViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    CategoryId = model.CategoryId,

                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                model.ImageFileNames = new List<string>(); // Initialize the list

                if (imageFiles != null && imageFiles.Any())
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0) // Check if the file is not empty
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fileStream);
                            }

                            var itemImage = new ProductImage
                            {
                                FileName = uniqueFileName,
                                ProductId = product.Id
                            };

                            _context.ProductImages.Add(itemImage);
                            model.ImageFileNames.Add(uniqueFileName); // Add filename to the model
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Products
                .Include(i => i.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.Name,
                ImageFileNames = item.Images.Select(img => img.FileName).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Products
                .Include(i => i.Category)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.Name,
                ImageFileNames = item.Images.Select(img => img.FileName).ToList()
            };

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", item.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model, List<IFormFile> ImageFileNames)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _context.Products
                        .Include(i => i.Images)
                        .FirstOrDefaultAsync(i => i.Id == id);

                    if (item == null)
                    {
                        return NotFound();
                    }

                    item.Name = model.Name;
                    item.Price = model.Price;
                    item.Description = model.Description;
                    item.CategoryId = model.CategoryId;

                    if (ImageFileNames != null && ImageFileNames.Any())
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        Directory.CreateDirectory(uploadsFolder);

                        foreach (var imageFile in ImageFileNames)
                        {
                            if (imageFile.Length > 0)
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await imageFile.CopyToAsync(fileStream);
                                }

                                var itemImage = new ProductImage
                                {
                                    FileName = uniqueFileName,
                                    ProductId = item.Id
                                };

                                _context.ProductImages.Add(itemImage);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        private bool ItemExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Controllers/ItemController.cs

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int itemId, string fileName)
        {
            var item = await _context.Products
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return Json(new { success = false });
            }

            var imageToDelete = item.Images.FirstOrDefault(img => img.FileName == fileName);

            if (imageToDelete != null)
            {
                // Remove the image from the database
                _context.ProductImages.Remove(imageToDelete);
                await _context.SaveChangesAsync();

                // Delete the file from the server
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        [HttpGet]
        public async Task<IActionResult> DeleteP(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Remove the item from the database.
            _context.Products.Remove(item);
            await _context.SaveChangesAsync();

            // Redirect to the Index view after deletion.
            return RedirectToAction(nameof(Index));
        }

    }

}
