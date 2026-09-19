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
    private readonly IConfiguration configuration;

    public OrderController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        this.db = db;
        this.userManager = userManager;
        this.configuration = configuration;

        Stripe.StripeConfiguration.ApiKey =
            configuration["Stripe:SecretKey"];
    }

    // GET: /Order/Checkout
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = await GetUserCart();

        if (cart is null || cart.CartItems.Count == 0)
        {
            TempData["OrderError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        if (cart.CartItems.Any(ci =>
            ci.Product is null ||
            ci.Quantity < 1 ||
            ci.Quantity > ci.Product.Quantity))
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
        var cart = await GetUserCart();

        if (cart is null || cart.CartItems.Count == 0)
        {
            TempData["OrderError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            return View(cart);
        }

        if (model.PaymentMethod != "Cash On Delivery" &&
            model.PaymentMethod != "Visa")
        {
            ModelState.AddModelError(
                nameof(model.PaymentMethod),
                "Invalid payment method.");

            return View(cart);
        }

        if (cart.CartItems.Any(ci =>
            ci.Product is null ||
            ci.Quantity < 1 ||
            ci.Quantity > ci.Product.Quantity))
        {
            TempData["OrderError"] =
                "One or more products do not have enough stock.";

            return RedirectToAction("Index", "Cart");
        }

        // =========================
        // VISA PAYMENT WITH STRIPE
        // =========================
        if (model.PaymentMethod == "Visa")
        {
            var sessionOptions =
                new Stripe.Checkout.SessionCreateOptions
                {
                    Mode = "payment",

                    PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },

                    SuccessUrl =
                        Url.Action(
                            nameof(PaymentSuccess),
                            "Order",
                            null,
                            Request.Scheme)
                        + "?session_id={CHECKOUT_SESSION_ID}",

                    CancelUrl =
                        Url.Action(
                            nameof(Checkout),
                            "Order",
                            null,
                            Request.Scheme),

                    CustomerEmail = User.Identity?.Name,

                    Metadata = new Dictionary<string, string>
                    {
                        ["UserId"] =
                            userManager.GetUserId(User)!,

                        ["FullName"] =
                            model.FullName,

                        ["Phone"] =
                            model.Phone,

                        ["City"] =
                            model.City,

                        ["Address"] =
                            model.Address,

                        ["PostalCode"] =
                            model.PostalCode ?? ""
                    },

                    LineItems =
                        new List<Stripe.Checkout.SessionLineItemOptions>()
                };

            foreach (var cartItem in cart.CartItems)
            {
                var lineItem =
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        Quantity = cartItem.Quantity,

                        PriceData =
                            new Stripe.Checkout.SessionLineItemPriceDataOptions
                            {
                                Currency = "egp",

                                UnitAmount =
                                    (long)(cartItem.Product.Price * 100),

                                ProductData =
                                    new Stripe.Checkout
                                        .SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = cartItem.Product.Title
                                    }
                            }
                    };

                sessionOptions.LineItems.Add(lineItem);
            }

            var sessionService =
                new Stripe.Checkout.SessionService();

            var session =
                await sessionService.CreateAsync(sessionOptions);

            return Redirect(session.Url);
        }

        // =========================
        // CASH ON DELIVERY
        // =========================
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var total = cart.CartItems.Sum(cartItem =>
                cartItem.Product.Price * cartItem.Quantity);

            var order = new Order
            {
                UserId = userManager.GetUserId(User)!,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                Status = "Pending",
                PaymentMethod = "Cash On Delivery",
                PaymentStatus = "Pending",

                FullName = model.FullName,
                Phone = model.Phone,
                City = model.City,
                Address = model.Address,
                PostalCode = model.PostalCode
            };

            foreach (var cartItem in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
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
        catch
        {
            await transaction.RollbackAsync();

            TempData["OrderError"] =
                "Something went wrong while placing your order.";

            return RedirectToAction("Index", "Cart");
        }
    }

    // GET: /Order/PaymentSuccess
    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<IActionResult> PaymentSuccess(string session_id)
    {
        if (string.IsNullOrWhiteSpace(session_id))
        {
            return BadRequest("Missing Stripe session id.");
        }

        var sessionService =
            new Stripe.Checkout.SessionService();

        var session =
            await sessionService.GetAsync(session_id);

        if (session.PaymentStatus != "paid")
        {
            TempData["OrderError"] =
                "Payment was not completed.";

            return RedirectToAction(nameof(Checkout));
        }

        var userId = userManager.GetUserId(User)!;

        // Prevent creating the same order more than once
        var existingOrder = await db.Orders
            .FirstOrDefaultAsync(o =>
                o.StripeSessionId == session_id);

        if (existingOrder is not null)
        {
            return RedirectToAction(
                nameof(Details),
                new { id = existingOrder.OrderId });
        }

        var cart = await GetUserCart();

        if (cart is null || cart.CartItems.Count == 0)
        {
            TempData["OrderError"] =
                "Your cart is empty or the order was already created.";

            return RedirectToAction(nameof(MyOrders));
        }

        if (cart.CartItems.Any(ci =>
            ci.Product is null ||
            ci.Quantity < 1 ||
            ci.Quantity > ci.Product.Quantity))
        {
            TempData["OrderError"] =
                "Some products are no longer available.";

            return RedirectToAction("Index", "Cart");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        try
        {
            var total = cart.CartItems.Sum(cartItem =>
                cartItem.Product.Price * cartItem.Quantity);

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                Status = "Pending",
                PaymentMethod = "Visa",
                PaymentStatus = "Paid",

                StripeSessionId = session_id,

                FullName = session.Metadata["FullName"],
                Phone = session.Metadata["Phone"],
                City = session.Metadata["City"],
                Address = session.Metadata["Address"],
                PostalCode = session.Metadata["PostalCode"]
            };

            foreach (var cartItem in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
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
        catch
        {
            await transaction.RollbackAsync();

            TempData["OrderError"] =
                "Payment succeeded, but there was an error creating the order.";

            return RedirectToAction(nameof(MyOrders));
        }
    }

    // Get current user's cart
    private async Task<Cart?> GetUserCart()
    {
        var userId = userManager.GetUserId(User);

        return await db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
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
    [Authorize(Roles = "Customer,Admin")]
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

        if (User.IsInRole("Admin"))
        {
            return View(order);
        }

        var userId = userManager.GetUserId(User);

        if (order.UserId != userId)
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

        var order = await db.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order is null)
        {
            return NotFound();
        }

        if (order.Status == "Delivered" ||
            order.Status == "Cancelled")
        {
            TempData["OrderError"] =
                "Delivered or cancelled orders cannot be updated.";

            return RedirectToAction(nameof(ManageOrders));
        }

        if (status == "Cancelled")
        {
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.Product is not null)
                {
                    orderItem.Product.Quantity += orderItem.Quantity;
                }
            }
        }

        order.Status = status;

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(ManageOrders));
    }
}