    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Admin.Models; // ADD THIS LINE
    using Microsoft.AspNetCore.Http; // ADD THIS LINE
    using System.Text.Json; // ADD THIS LINE

    namespace Admin.Controllers
    {
        public class OrderController : Controller
        {
            private readonly ProductDBContext _context;

            public OrderController(ProductDBContext context)
            {
                _context = context;
            }

            // Checkout Page
            [HttpGet]
            public IActionResult Checkout()
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login to checkout";
                    return RedirectToAction("Login", "Customer");
                }

                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["error"] = "Your cart is empty";
                    return RedirectToAction("ViewCart", "Cart");
                }

                var checkoutModel = new CheckoutModel
                {
                    Cart = cart,
                    Recipient = new Recipient
                    {
                        Date = CalculateDeliveryDate()
                    }
                };

                ViewBag.OccasionMessages = _context.OccasionMessages.ToList();
                return View(checkoutModel);
            }

            // Process Checkout
            [HttpPost]
            public IActionResult ProcessCheckout(CheckoutModel model)
            {
                try
                {
                    var customerId = HttpContext.Session.GetInt32("CustomerId");
                    if (customerId == null)
                    {
                        TempData["error"] = "Please login to checkout";
                        return RedirectToAction("Login", "Customer");
                    }

                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .FirstOrDefault(c => c.CustomerId == customerId);

                    if (cart == null || !cart.CartItems.Any())
                    {
                        TempData["error"] = "Your cart is empty";
                        return RedirectToAction("ViewCart", "Cart");
                    }

                    // Validate recipient information
                    if (string.IsNullOrEmpty(model.Recipient.Name) ||
                        string.IsNullOrEmpty(model.Recipient.Address) ||
                        model.Recipient.PhoneNo == 0)
                    {
                        TempData["error"] = "Please fill all required recipient information";
                        ViewBag.OccasionMessages = _context.OccasionMessages.ToList();
                        return View("Checkout", model);
                    }

                    // Validate delivery date
                    if (model.Recipient.Date < DateTime.Today)
                    {
                        TempData["error"] = "Delivery date cannot be in the past";
                        ViewBag.OccasionMessages = _context.OccasionMessages.ToList();
                        return View("Checkout", model);
                    }

                    // Create order
                    var order = new Order
                    {
                        CustomerId = customerId.Value,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.TotalAmount,
                        Status = "Pending",
                        PaymentStatus = "Paid", // Simulate payment success
                        TransactionId = Guid.NewGuid().ToString().Substring(0, 15).ToUpper()
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    // Add order items
                    foreach (var cartItem in cart.CartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = cartItem.Price
                        };
                        _context.OrderItems.Add(orderItem);
                    }

                    // Add recipient information
                    model.Recipient.OrderId = order.OrderId;

                    // Set custom message if provided
                    if (!string.IsNullOrEmpty(model.CustomMessage))
                    {
                        model.Recipient.CustomMessage = model.CustomMessage;
                    }

                    _context.Recipients.Add(model.Recipient);

                    // Clear cart
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.Carts.Remove(cart);

                    _context.SaveChanges();

                    TempData["success"] = $"Order placed successfully! Your Order ID is #{order.OrderId}";
                    return RedirectToAction("OrderConfirmation", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Error processing order: {ex.Message}";
                    ViewBag.OccasionMessages = _context.OccasionMessages.ToList();
                    return View("Checkout", model);
                }
            }

            // Order Confirmation Page
            public IActionResult OrderConfirmation(int id)
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Recipient)
                    .Include(o => o.Customer)
                    .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);

                if (order == null)
                {
                    TempData["error"] = "Order not found";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }

            // View Customer Orders
            public IActionResult MyOrders()
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login to view your orders";
                    return RedirectToAction("Login", "Customer");
                }

                var orders = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Recipient)
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                return View(orders);
            }

            // Order Details
            public IActionResult OrderDetails(int id)
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Recipient)
                    .Include(o => o.Customer)
                    .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);

                if (order == null)
                {
                    TempData["error"] = "Order not found";
                    return RedirectToAction("MyOrders");
                }

                return View(order);
            }

            // Cancel Order
            [HttpPost]
            public IActionResult CancelOrder(int id)
            {
                try
                {
                    var customerId = HttpContext.Session.GetInt32("CustomerId");
                    var order = _context.Orders
                        .FirstOrDefault(o => o.OrderId == id && o.CustomerId == customerId);

                    if (order == null)
                    {
                        return Json(new { success = false, message = "Order not found" });
                    }

                    if (order.Status == "Delivered" || order.Status == "Shipped")
                    {
                        return Json(new { success = false, message = "Cannot cancel order that is already shipped or delivered" });
                    }

                    order.Status = "Cancelled";
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Order cancelled successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Error: {ex.Message}" });
                }
            }

            // Helper method to calculate delivery date based on working hours
            private DateTime CalculateDeliveryDate()
            {
                var now = DateTime.Now;
                var currentTime = now.TimeOfDay;
                var workingStart = new TimeSpan(9, 0, 0); // 9:00 AM
                var workingEnd = new TimeSpan(21, 0, 0); // 9:00 PM
                var cutoffTime = new TimeSpan(16, 0, 0); // 4:00 PM for same-day delivery

                // If current time is within working hours and before 4 PM, deliver today
                if (currentTime >= workingStart && currentTime < cutoffTime)
                {
                    // Deliver within 5 hours - same day
                    return now.AddHours(5);
                }
                else
                {
                    // Deliver next day at 2 PM
                    var nextDay = now.AddDays(1);
                    return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 14, 0, 0);
                }
            }

        }
    }