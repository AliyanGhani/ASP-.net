using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Admin.Models;
using Microsoft.AspNetCore.Http;

namespace Admin.Controllers
{
    public class CartController : Controller
    {
        private readonly ProductDBContext _context;

        public CartController(ProductDBContext context)
        {
            _context = context;
        }

        // View Cart Page
        public IActionResult ViewCart()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["error"] = "Please login to view your cart";
                return RedirectToAction("Login", "Customer");
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerId == customerId);

            return View(cart);
        }

        // SIMPLE ADD TO CART
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login to add items to cart";
                    return RedirectToAction("Login", "Customer");
                }

                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    TempData["error"] = "Product not found";
                    return RedirectToAction("Index", "Home");
                }

                // Get or create cart
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        CustomerId = customerId.Value,
                        CreatedDate = DateTime.Now
                    };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }

                // Check if item exists
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Pro_price
                    };
                    _context.CartItems.Add(cartItem);
                }

                _context.SaveChanges();
                UpdateCartTotal(cart.CartId);

                TempData["success"] = $"{product.Pro_name} added to cart successfully!";
                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // SIMPLE UPDATE QUANTITY
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login";
                    return RedirectToAction("Login", "Customer");
                }

                var cartItem = _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.CustomerId == customerId);

                if (cartItem != null && quantity > 0 && quantity <= 10)
                {
                    cartItem.Quantity = quantity;
                    _context.SaveChanges();
                    UpdateCartTotal(cartItem.CartId);
                    TempData["success"] = "Cart updated successfully";
                }
                else
                {
                    TempData["error"] = "Invalid quantity";
                }

                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("ViewCart");
            }
        }

        // SIMPLE REMOVE FROM CART
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login";
                    return RedirectToAction("Login", "Customer");
                }

                var cartItem = _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.CustomerId == customerId);

                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
                    UpdateCartTotal(cartItem.CartId);
                    TempData["success"] = "Item removed from cart";
                }
                else
                {
                    TempData["error"] = "Item not found";
                }

                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("ViewCart");
            }
        }

        // SIMPLE CLEAR CART
        [HttpPost]
        public IActionResult ClearCart()
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["error"] = "Please login";
                    return RedirectToAction("Login", "Customer");
                }

                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.CustomerId == customerId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.SaveChanges();
                    TempData["success"] = "Cart cleared successfully";
                }

                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("ViewCart");
            }
        }
        // Get Cart Count (AJAX)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return Json(new { count = 0 });
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.CustomerId == customerId);

            var count = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
            return Json(new { count = count });
        }

        // Helper Method
        private void UpdateCartTotal(int cartId)
        {
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.CartId == cartId);

            if (cart != null)
            {
                cart.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
                _context.SaveChanges();
            }
        }
    }
}