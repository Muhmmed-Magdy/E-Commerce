
using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;

        public ProductController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var products = context.Products
                .Include(p => p.Category)
                .ToList();

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound("this product is not here");

            return View(product);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = context.Categories.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = context.Categories.ToList();
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

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
                return NotFound("this product is not here");

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

            ViewBag.Categories = context.Categories.ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = context.Categories.ToList();
                return View(model);
            }

            var product = context.Products.Find(model.ProductId);

            if (product == null)
                return NotFound( "this product is not here");

            product.Title = model.Title;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Quantity = model.Quantity;
            product.CategoryId = model.CategoryId;

            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
                return NotFound("this product is not here");

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
                return NotFound("this product is not here");

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Search(string search)
        {
            var products = context.Products
                .Include(p => p.Category)
                .Where(p => p.Title.Contains(search))
                .ToList();

            return View("Index", products);
        }
    }
}
