using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductDBContext _context;

        public HomeController(ProductDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ✅ Get 3 random categories
            var randomCategories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .ToList();

            // ✅ NEW: Get 3 random products using your actual model
            var randomProducts = _context.Products
                .Where(p => p.IsAvailable) // Your property name is IsAvailable
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .ToList();

            ViewBag.RandomProducts = randomProducts;
            return View(randomCategories);
        }

        // ✅ FIXED SHOP ACTION WITH CATEGORIES
        public IActionResult Shop()
        {
            // Get all available products
            var products = _context.Products
                .Where(p => p.IsAvailable)
                .ToList();

            // ✅ FIXED: Get all active categories for the filter dropdown
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .ToList();

            // ✅ FIXED: Pass categories to view using ViewBag
            ViewBag.Categories = categories;

            // Debugging: Check products and categories
            foreach (var product in products)
            {
                System.Diagnostics.Debug.WriteLine($"Product: {product.Pro_name}, Image: {product.Pro_imagename}, Price: {product.Pro_price}");
            }

            System.Diagnostics.Debug.WriteLine($"Total Categories: {categories.Count}");

            return View(products);
        }

        public IActionResult ProductsByCategory(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var products = _context.Products
                .Where(p => p.Pro_category == category.CategoryName && p.IsAvailable)
                .ToList();

            ViewBag.Category = category.CategoryName;
            ViewBag.CategoryDescription = category.Description;
            return View(products);
        }

        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        // ✅ FLORIST REGISTRATION ACTIONS
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterFlorist(FloristRegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all required fields correctly.";
                return View("Registration", model);
            }

            try
            {
                // File handling
                if (model.BusinessLicense != null && model.BusinessLicense.Length > 0)
                {
                    var licensesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "licenses");
                    Directory.CreateDirectory(licensesPath);

                    var filePath = Path.Combine(licensesPath, model.BusinessLicense.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.BusinessLicense.CopyToAsync(stream);
                    }
                }

                if (model.TaxId != null && model.TaxId.Length > 0)
                {
                    var taxIdsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "taxids");
                    Directory.CreateDirectory(taxIdsPath);

                    var filePath = Path.Combine(taxIdsPath, model.TaxId.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.TaxId.CopyToAsync(stream);
                    }
                }

                // TODO: Save to database
                // _context.Florists.Add(new Florist { ... });
                // await _context.SaveChangesAsync();

                TempData["success"] = "Registration successful! We will contact you within 3 business days.";
                return RedirectToAction("RegistrationSuccess");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error during registration. Please try again.";
                return View("Registration", model);
            }
        }

        public IActionResult RegistrationSuccess()
        {
            return View();
        }
    }
}