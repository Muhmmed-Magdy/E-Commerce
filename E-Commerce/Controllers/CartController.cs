using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Customer")]
public class CartController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> userManager;

    public CartController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    // GET: /Cart
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        var cart = await db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return View(cart);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        if (quantity < 1)
        {
            TempData["CartError"] = "Quantity must be at least 1.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        var product = await db.Products.FindAsync(productId);

        if (product is null)
        {
            return NotFound();
        }

        if (product.Quantity < 1)
        {
            TempData["CartError"] = "This product is out of stock.";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        var userId = userManager.GetUserId(User)!;

        var cart = await db.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }

        var item = await db.CartItems
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cart.CartId &&
                ci.ProductId == productId);

        if (item is null)
        {
            if (quantity > product.Quantity)
            {
                TempData["CartError"] =
                    $"Only {product.Quantity} item(s) are available.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            db.CartItems.Add(new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            var newQuantity = item.Quantity + quantity;

            if (newQuantity > product.Quantity)
            {
                TempData["CartError"] =
                    $"Only {product.Quantity} item(s) are available.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            item.Quantity = newQuantity;
        }

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/UpdateQuantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        if (quantity < 1)
        {
            TempData["CartError"] = "Quantity must be at least 1.";
            return RedirectToAction(nameof(Index));
        }

        var userId = userManager.GetUserId(User);

        var item = await db.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci =>
                ci.CartItemId == cartItemId &&
                ci.Cart.UserId == userId);

        if (item is null)
        {
            return NotFound();
        }

        if (quantity > item.Product.Quantity)
        {
            TempData["CartError"] =
                $"Only {item.Product.Quantity} item(s) are available.";
            return RedirectToAction(nameof(Index));
        }

        item.Quantity = quantity;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/RemoveFromCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userId = userManager.GetUserId(User);

        var item = await db.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci =>
                ci.CartItemId == cartItemId &&
                ci.Cart.UserId == userId);

        if (item is null)
        {
            return NotFound();
        }

        db.CartItems.Remove(item);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
