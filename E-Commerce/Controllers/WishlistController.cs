using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Customer")]
public class WishlistController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> userManager;

    public WishlistController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    // GET: /Wishlist
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        var items = await db.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        return View(items);
    }

    // POST: /Wishlist/AddToWishlist
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var product = await db.Products.FindAsync(productId);

        if (product is null)
        {
            return NotFound();
        }

        var userId = userManager.GetUserId(User)!;

        var alreadyExists = await db.Wishlists.AnyAsync(w =>
            w.UserId == userId &&
            w.ProductId == productId);

        if (!alreadyExists)
        {
            db.Wishlists.Add(new Wishlist
            {
                UserId = userId,
                ProductId = productId
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Product");
    }

    // POST: /Wishlist/RemoveFromWishlist
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
    {
        var userId = userManager.GetUserId(User);

        var item = await db.Wishlists
            .FirstOrDefaultAsync(w =>
                w.WishlistId == wishlistId &&
                w.UserId == userId);

        if (item is null)
        {
            return NotFound();
        }

        db.Wishlists.Remove(item);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}

