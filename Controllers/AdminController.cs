using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly ProductDBContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public AdminController(ProductDBContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        // ========== ADMIN AUTHENTICATION ==========
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["error"] = "Please enter both username and password";
                return View();
            }

            var adminUsername = _configuration["AdminCredentials:Username"];
            var adminPassword = _configuration["AdminCredentials:Password"];

            if (username == adminUsername && password == adminPassword)
            {
                HttpContext.Session.SetInt32("AdminId", 1);
                HttpContext.Session.SetString("AdminName", "Administrator");
                HttpContext.Session.SetString("AdminUsername", username);

                TempData["success"] = "Welcome back, Administrator!";
                return RedirectToAction("Dashboard");
            }

            TempData["error"] = "Invalid username or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["success"] = "Logged out successfully";
            return RedirectToAction("Login");
        }

        // ========== ADMIN DASHBOARD ==========
        public IActionResult Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            // Use ViewBag instead of model
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalRevenue = _context.Orders.Where(o => o.PaymentStatus == "Paid").Sum(o => o.TotalAmount);
            ViewBag.RecentOrders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            return View();
        }
        

        // ========== PRODUCT MANAGEMENT ==========
        [HttpGet]
        public IActionResult AddProduct()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            ViewBag.Categories = _context.Categories
        .Where(c => c.IsActive)
        .OrderBy(c => c.CategoryName)
        .ToList();
            return View(new Products());
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        [RequestSizeLimit(10485760)]
        public IActionResult AddProduct(Products c)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            if (ModelState.IsValid)
            {
                string filename = "default-product.jpg";

                if (c.Pro_img != null && c.Pro_img.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "Content/Images");
                    filename = Guid.NewGuid().ToString() + "_" + c.Pro_img.FileName;
                    string filePath = Path.Combine(uploadFolder, filename);
                    string extension = Path.GetExtension(c.Pro_img.FileName).ToLower();

                    if (extension == ".jfif" || extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
                    {
                        if (c.Pro_img.Length <= 10485760)
                        {
                            if (!Directory.Exists(uploadFolder))
                                Directory.CreateDirectory(uploadFolder);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                c.Pro_img.CopyTo(fileStream);
                        }
                        else
                        {
                            TempData["error"] = "File size must be less than 10MB";
                            return View(c);
                        }
                    }
                    else
                    {
                        TempData["extension_error"] = "Only image files are allowed (JPG, JPEG, PNG, WEBP, JFIF)";
                        return View(c);
                    }
                }

                Products prod = new Products
                {
                    Pro_name = c.Pro_name,
                    Pro_des = c.Pro_des,
                    Pro_category = c.Pro_category,
                    Pro_price = c.Pro_price,
                    Pro_imagename = filename,
                    IsAvailable = true
                };

                _context.Products.Add(prod);
                _context.SaveChanges();
                TempData["success"] = "Product added successfully";
                return RedirectToAction("Products");
            }
            return View(c);
        }

        public IActionResult Products()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            return View(_context.Products.ToList());
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            var product = _context.Products.Find(id);
            if (product == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction("Products");
            }
            ViewBag.Categories = _context.Categories
       .Where(c => c.IsActive)
       .OrderBy(c => c.CategoryName)
       .ToList();

            return View(product);
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        [RequestSizeLimit(10485760)]
        public IActionResult EditProduct(Products c)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            if (!ModelState.IsValid)
                return View(c);

            try
            {
                var existingProduct = _context.Products.Find(c.Pro_id);
                if (existingProduct == null)
                {
                    TempData["error"] = "Product not found";
                    return RedirectToAction("Products");
                }

                string filename = existingProduct.Pro_imagename;

                if (c.Pro_img != null && c.Pro_img.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "Content/Images");
                    filename = Guid.NewGuid().ToString() + "_" + c.Pro_img.FileName;
                    string filePath = Path.Combine(uploadFolder, filename);
                    string extension = Path.GetExtension(c.Pro_img.FileName).ToLower();

                    if (extension == ".jfif" || extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
                    {
                        if (c.Pro_img.Length <= 10485760)
                        {
                            if (!Directory.Exists(uploadFolder))
                                Directory.CreateDirectory(uploadFolder);

                            if (!string.IsNullOrEmpty(existingProduct.Pro_imagename) && existingProduct.Pro_imagename != "default-product.jpg")
                            {
                                string oldFilePath = Path.Combine(uploadFolder, existingProduct.Pro_imagename);
                                if (System.IO.File.Exists(oldFilePath))
                                    System.IO.File.Delete(oldFilePath);
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                c.Pro_img.CopyTo(fileStream);
                        }
                        else
                        {
                            TempData["error"] = "File size must be less than 10MB";
                            return View(c);
                        }
                    }
                    else
                    {
                        TempData["extension_error"] = "Only image files are allowed (JPG, JPEG, PNG, WEBP, JFIF)";
                        return View(c);
                    }
                }

                existingProduct.Pro_name = c.Pro_name;
                existingProduct.Pro_des = c.Pro_des;
                existingProduct.Pro_category = c.Pro_category;
                existingProduct.Pro_price = c.Pro_price;
                existingProduct.Pro_imagename = filename;

                _context.Products.Update(existingProduct);
                _context.SaveChanges();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error updating product: {ex.Message}";
                return View(c);
            }
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            var product = _context.Products.Find(id);
            if (product == null)
            {
                TempData["error"] = "Product not found";
                return RedirectToAction("Products");
            }
            return View(product);
        }

        [HttpPost]
        [ActionName("DeleteProduct")]
        public IActionResult DeleteProductConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                {
                    TempData["error"] = "Product not found";
                    return RedirectToAction("Products");
                }

                if (!string.IsNullOrEmpty(product.Pro_imagename) && product.Pro_imagename != "default-product.jpg")
                {
                    string filePath = Path.Combine(_environment.WebRootPath, "Content/Images", product.Pro_imagename);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["success"] = "Product deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction("Products");
        }

        // ========== CUSTOMER MANAGEMENT ==========
        public IActionResult Customers()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            var customers = _context.Customers
                .Include(c => c.Orders)
                .OrderByDescending(c => c.Custid)
                .ToList();

            return View(customers);
        }
        [HttpGet]
        public IActionResult GetCustomerStatistics(int customerId)
        {
            try
            {
                var customer = _context.Customers
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefault(c => c.Custid == customerId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                var orders = customer.Orders;
                var totalOrders = orders.Count;
                var pendingOrders = orders.Count(o => o.Status == "Pending");
                var confirmedOrders = orders.Count(o => o.Status == "Confirmed");
                var deliveredOrders = orders.Count(o => o.Status == "Delivered");
                var cancelledOrders = orders.Count(o => o.Status == "Cancelled");

                var totalSpent = orders
                    .Where(o => o.PaymentStatus == "Paid")
                    .Sum(o => o.TotalAmount);

                var averageOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;

                // Last order details
                var lastOrder = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault();
                var lastOrderDate = lastOrder?.OrderDate.ToString("MMM dd, yyyy") ?? "No orders";
                var lastOrderAmount = lastOrder?.TotalAmount ?? 0;

                // Most ordered product category
                var popularCategory = orders
                    .SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.Product.Pro_category)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .FirstOrDefault();

                // Order frequency (days between orders)
                var orderDates = orders.Select(o => o.OrderDate).OrderBy(d => d).ToList();
                var averageDaysBetweenOrders = 0.0;
                if (orderDates.Count > 1)
                {
                    var totalDays = (orderDates.Last() - orderDates.First()).TotalDays;
                    averageDaysBetweenOrders = totalDays / (orderDates.Count - 1);
                }

                // Total items ordered
                var totalItemsOrdered = orders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity));

                // Payment statistics
                var successfulPayments = orders.Count(o => o.PaymentStatus == "Paid");
                var failedPayments = orders.Count(o => o.PaymentStatus == "Failed");

                return Json(new
                {
                    success = true,
                    // Basic stats
                    totalOrders = totalOrders,
                    pendingOrders = pendingOrders,
                    confirmedOrders = confirmedOrders,
                    deliveredOrders = deliveredOrders,
                    cancelledOrders = cancelledOrders,
                    totalSpent = totalSpent,
                    averageOrderValue = Math.Round(averageOrderValue, 2),

                    // Customer behavior
                    lastOrderDate = lastOrderDate,
                    lastOrderAmount = lastOrderAmount,
                    popularCategory = popularCategory?.Category ?? "No data",
                    popularCategoryCount = popularCategory?.Count ?? 0,
                    averageDaysBetweenOrders = Math.Round(averageDaysBetweenOrders, 1),

                    // Customer info
                    customerName = $"{customer.F_name} {customer.L_name}",
                    customerPhone = customer.P_no.ToString(),
                    customerAddress = customer.Address,
                    customerSince = customer.Dob.ToString("MMM yyyy"),
                    totalItemsOrdered = totalItemsOrdered,

                    // Payment info
                    successfulPayments = successfulPayments,
                    failedPayments = failedPayments
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== ORDER MANAGEMENT ==========
        public IActionResult Orders()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Recipient)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login first" });

            try
            {
                var order = _context.Orders.Find(orderId);
                if (order == null)
                    return Json(new { success = false, message = "Order not found" });

                order.Status = status;
                _context.SaveChanges();
                return Json(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ========== OCCASION MESSAGES MANAGEMENT ==========
        public IActionResult OccasionMessages()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            return View(_context.OccasionMessages.ToList());
        }

        [HttpGet]
        public IActionResult AddOccasionMessage()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            return View();
        }

        [HttpPost]
        public IActionResult AddOccasionMessage(OccasionMessage message)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            if (ModelState.IsValid)
            {
                _context.OccasionMessages.Add(message);
                _context.SaveChanges();
                TempData["success"] = "Occasion message added successfully";
                return RedirectToAction("OccasionMessages");
            }
            return View(message);
        }

        // ========== HELPER METHODS ==========
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }

        private IActionResult RedirectToLogin()
        {
            TempData["error"] = "Please login first";
            return RedirectToAction("Login");
        }
        public IActionResult Categories()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            return View(_context.Categories.OrderBy(c => c.CategoryName).ToList());
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();
            return View(new Category());
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        [RequestSizeLimit(10485760)]
        public IActionResult AddCategory(Category category)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            if (ModelState.IsValid)
            {
                string filename = "default-category.jpg";

                if (category.CategoryImage != null && category.CategoryImage.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "Content/CategoryImages");
                    filename = Guid.NewGuid().ToString() + "_" + category.CategoryImage.FileName;
                    string filePath = Path.Combine(uploadFolder, filename);
                    string extension = Path.GetExtension(category.CategoryImage.FileName).ToLower();

                    if (extension == ".jfif" || extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
                    {
                        if (category.CategoryImage.Length <= 10485760)
                        {
                            if (!Directory.Exists(uploadFolder))
                                Directory.CreateDirectory(uploadFolder);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                category.CategoryImage.CopyTo(fileStream);
                        }
                        else
                        {
                            TempData["error"] = "File size must be less than 10MB";
                            return View(category);
                        }
                    }
                    else
                    {
                        TempData["error"] = "Only image files are allowed (JPG, JPEG, PNG, WEBP, JFIF)";
                        return View(category);
                    }
                }

                category.ImageName = filename;
                category.CreatedDate = DateTime.Now;

                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["success"] = "Category added successfully";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                TempData["error"] = "Category not found";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 10485760)]
        [RequestSizeLimit(10485760)]
        public IActionResult EditCategory(Category category)
        {
            if (!IsAdminLoggedIn())
                return RedirectToLogin();

            if (!ModelState.IsValid)
                return View(category);

            try
            {
                var existingCategory = _context.Categories.Find(category.CategoryId);
                if (existingCategory == null)
                {
                    TempData["error"] = "Category not found";
                    return RedirectToAction("Categories");
                }

                string filename = existingCategory.ImageName;

                if (category.CategoryImage != null && category.CategoryImage.Length > 0)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "Content/CategoryImages");
                    filename = Guid.NewGuid().ToString() + "_" + category.CategoryImage.FileName;
                    string filePath = Path.Combine(uploadFolder, filename);
                    string extension = Path.GetExtension(category.CategoryImage.FileName).ToLower();

                    if (extension == ".jfif" || extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".webp")
                    {
                        if (category.CategoryImage.Length <= 10485760)
                        {
                            if (!Directory.Exists(uploadFolder))
                                Directory.CreateDirectory(uploadFolder);

                            // Delete old image if exists
                            if (!string.IsNullOrEmpty(existingCategory.ImageName) && existingCategory.ImageName != "default-category.jpg")
                            {
                                string oldFilePath = Path.Combine(uploadFolder, existingCategory.ImageName);
                                if (System.IO.File.Exists(oldFilePath))
                                    System.IO.File.Delete(oldFilePath);
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                                category.CategoryImage.CopyTo(fileStream);
                        }
                        else
                        {
                            TempData["error"] = "File size must be less than 10MB";
                            return View(category);
                        }
                    }
                    else
                    {
                        TempData["error"] = "Only image files are allowed (JPG, JPEG, PNG, WEBP, JFIF)";
                        return View(category);
                    }
                }

                existingCategory.CategoryName = category.CategoryName;
                existingCategory.Description = category.Description;
                existingCategory.ImageName = filename;
                existingCategory.IsActive = category.IsActive;

                _context.Categories.Update(existingCategory);
                _context.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error updating category: {ex.Message}";
                return View(category);
            }
        }

        [HttpPost]
        public IActionResult ToggleCategoryStatus(int id)  // Remove [FromBody]
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login first" });

            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                category.IsActive = !category.IsActive;
                _context.SaveChanges();

                var status = category.IsActive ? "activated" : "deactivated";
                return Json(new { success = true, message = $"Category {status} successfully", isActive = category.IsActive });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)  // Remove [FromBody]
        {
            if (!IsAdminLoggedIn())
                return Json(new { success = false, message = "Please login first" });

            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                // Check if category has products
                var hasProducts = _context.Products.Any(p => p.Pro_category == category.CategoryName);
                if (hasProducts)
                    return Json(new { success = false, message = "Cannot delete category that has products. Please reassign or delete products first." });

                // Delete image file
                if (!string.IsNullOrEmpty(category.ImageName) && category.ImageName != "default-category.jpg")
                {
                    string filePath = Path.Combine(_environment.WebRootPath, "Content/CategoryImages", category.ImageName);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();
                return Json(new { success = true, message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
    
    // ========== CATEGORY MANAGEMENT ==========


        //public class DashboardStats
        //{
        //    public int TotalProducts { get; set; }
        //    public int TotalCustomers { get; set; }
        //    public int TotalOrders { get; set; }
        //    public decimal TotalRevenue { get; set; }
        //    public List<Order> RecentOrders {b get; set; } = new List<Order>();
        //}
    }