using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Customer,Admin")]
public class OrderController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> userManager;

    public OrderController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    // GET: /Order/Checkout
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var userId = userManager.GetUserId(User);

        var cart = await db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || cart.CartItems.Count == 0)
        {
            TempData["OrderError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        if (cart.CartItems.Any(ci => ci.Quantity > ci.Product.Quantity))
        {
            TempData["OrderError"] =
                "One or more products do not have enough stock.";
            return RedirectToAction("Index", "Cart");
        }

        return View(cart);
    }

    // POST: /Order/Checkout
    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!string.Equals(
                model.PaymentMethod,
                "Cash On Delivery",
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.PaymentMethod),
                "Only Cash On Delivery is currently supported.");

            return View(model);
        }

        var userId = userManager.GetUserId(User)!;

        await using var transaction = await db.Database.BeginTransactionAsync();

        var cart = await db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || cart.CartItems.Count == 0)
        {
            TempData["OrderError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        foreach (var cartItem in cart.CartItems)
        {
            if (cartItem.Quantity < 1 ||
                cartItem.Quantity > cartItem.Product.Quantity)
            {
                TempData["OrderError"] =
                    $"Not enough stock for {cartItem.Product.Title}.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Recalculate the total on the server.
        var total = cart.CartItems.Sum(ci =>
            ci.Product.Price * ci.Quantity);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            Status = "Pending",
            PaymentMethod = "Cash On Delivery",
            PaymentStatus = "Pending"
        };

        foreach (var cartItem in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                // Store the price at purchase time.
                Price = cartItem.Product.Price
            });

            cartItem.Product.Quantity -= cartItem.Quantity;
        }

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cart.CartItems);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return RedirectToAction(
            nameof(Details),
            new { id = order.OrderId });
    }

    // GET: /Order/MyOrders
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var userId = userManager.GetUserId(User);

        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // GET: /Order/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null)
        {
            return NotFound();
        }

        var userId = userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        // A customer can only see their own orders.
        if (!isAdmin && order.UserId != userId)
        {
            return Forbid();
        }

        return View(order);
    }

    // GET: /Order/ManageOrders
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> ManageOrders(string? status)
    {
        var query = db.Orders
            .Include(o => o.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // POST: /Order/UpdateStatus
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[]
        {
            "Pending",
            "Confirmed",
            "Shipped",
            "Delivered",
            "Cancelled"
        };

        if (!allowedStatuses.Contains(status))
        {
            return BadRequest("Invalid order status.");
        }

        var order = await db.Orders.FindAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        order.Status = status;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(ManageOrders));
    }
}
