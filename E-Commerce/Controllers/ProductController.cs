using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly UserManager<ApplicationUser> userManager;

        public ProductController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.environment = environment;
            this.userManager = userManager;
        }

        // Customer and Admin
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId)
        {
            var productsQuery = context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Search by product title
            if (!string.IsNullOrWhiteSpace(search))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Title.Contains(search));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            var products = await productsQuery.ToListAsync();

            // Load categories for dropdown
            ViewBag.Categories = await context.Categories
                .ToListAsync();

            // Keep search and category values in the view
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            // Wishlist products
            var wishlistProductIds = new HashSet<int>();

            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Customer"))
            {
                var userId = userManager.GetUserId(User);

                wishlistProductIds = (await context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.ProductId)
                    .ToListAsync())
                    .ToHashSet();
            }

            ViewBag.WishlistProductIds = wishlistProductIds;

            return View(products);
        }

        // Customer and Admin
        public async Task<IActionResult> Details(int id)
        {
            var product = await context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("this product is not here");
            }

            return View(product);
        }

        // Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await context.Categories
                .ToListAsync();

            return View();
        }

        // Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await context.Categories
                    .ToListAsync();

                return View(model);
            }

            var product = new Product
            {
                Title = model.Title,
                Price = model.Price,
                Description = model.Description,
                Quantity = model.Quantity,
                CategoryId = model.CategoryId
            };

            if (model.Image != null)
            {
                string folder = Path.Combine(
                    environment.WebRootPath,
                    "images",
                    "products"
                );

                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString()
                                  + Path.GetExtension(model.Image.FileName);

                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(
                    filePath,
                    FileMode.Create
                );

                await model.Image.CopyToAsync(stream);

                product.ImagePath = "/images/products/" + fileName;
            }

            context.Products.Add(product);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound("this product is not here");
            }

            var model = new EditProductViewModel
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                ImagePath = product.ImagePath
            };

            ViewBag.Categories = await context.Categories
                .ToListAsync();

            return View(model);
        }

        // Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await context.Categories
                    .ToListAsync();

                return View(model);
            }

            var product = await context.Products
                .FindAsync(model.ProductId);

            if (product == null)
            {
                return NotFound("this product is not here");
            }

            product.Title = model.Title;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Quantity = model.Quantity;
            product.CategoryId = model.CategoryId;

            // Change image only if user selected a new image
            if (model.Image != null)
            {
                string folder = Path.Combine(
                    environment.WebRootPath,
                    "images",
                    "products"
                );

                Directory.CreateDirectory(folder);

                // Delete old image
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string oldImagePath = Path.Combine(
                        environment.WebRootPath,
                        product.ImagePath.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                string fileName = Guid.NewGuid().ToString()
                                  + Path.GetExtension(model.Image.FileName);

                string filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(
                    filePath,
                    FileMode.Create
                );

                await model.Image.CopyToAsync(stream);

                product.ImagePath = "/images/products/" + fileName;
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await context.Products
                .FindAsync(id);

            if (product == null)
            {
                return NotFound("this product is not here");
            }

            return View(product);
        }

        // Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await context.Products
                .FindAsync(id);

            if (product == null)
            {
                return NotFound("this product is not here");
            }

            // Delete product image
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                string imagePath = Path.Combine(
                    environment.WebRootPath,
                    product.ImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Customer and Admin
        public IActionResult Search(
            string? search,
            int? categoryId)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    search = search,
                    categoryId = categoryId
                });
        }
    }
}