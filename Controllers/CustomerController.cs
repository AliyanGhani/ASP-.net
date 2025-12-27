using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admin.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ProductDBContext _context;
        
        public CustomerController(ProductDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if email/phone already exists
                var existingCustomer = _context.Customers
                    .FirstOrDefault(c => c.P_no == customer.P_no);

                if (existingCustomer != null)
                {
                    ModelState.AddModelError("P_no", "Phone number already registered");
                    return View(customer);
                }

                _context.Customers.Add(customer);
                _context.SaveChanges();

                TempData["success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            return View(customer);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(long phoneNumber)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.P_no == phoneNumber);

            if (customer != null)
            {
                HttpContext.Session.SetInt32("CustomerId", customer.Custid);
                HttpContext.Session.SetString("CustomerName", $"{customer.F_name} {customer.L_name}");

                TempData["success"] = $"Welcome back, {customer.F_name}!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid phone number");
            return View();
        }
        [HttpGet]
        public IActionResult IsLoggedIn()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            return Json(new { isLoggedIn = customerId != null });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["success"] = "Logged out successfully";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult MyAccount()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["error"] = "Please login to access your account";
                return RedirectToAction("Login");
            }

            var customer = _context.Customers
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .Include(c => c.Orders)
            .ThenInclude(o => o.Recipient)
        .FirstOrDefault(c => c.Custid == customerId);

            if (customer == null)
            {
                TempData["error"] = "Customer not found";
                return RedirectToAction("Login");
            }


            return View(customer);
        }
        public IActionResult OrderDetails(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["error"] = "Please login to view order details";
                return RedirectToAction("Login");
            }

            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Recipient)
                .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);

            if (order == null)
            {
                TempData["error"] = "Order not found";
                return RedirectToAction("MyAccount");
            }

            return View(order);
        }
    }
}